using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using CoreWingedBoots = MegaCrit.Sts2.Core.Models.Relics.WingedBoots;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class Key : ModRelicTemplate
{
    private const string RoomsKey = "Rooms";
    private const int InitialKeyChargeRewardChancePercent = 10;
    private const int KeyChargeRewardChanceIncreasePercent = 8;
    private const int InitialBoxChancePercent = 5;
    private const int BoxChanceIncreasePercent = 6;
    private const string OpenCofferWithKeyRewardDescriptionKey = "ARKNIGHTS_MIZUKI_REWARD_DRIFTING_COFFER_OPEN_WITH_KEY";
    private const string BreakCofferWithHammerRewardDescriptionKey = "ARKNIGHTS_MIZUKI_REWARD_DRIFTING_COFFER_BREAK_WITH_HAMMER";

    public const int DefaultCharges = 1;

    private int _chargesRemaining = DefaultCharges;

    private int _keyChargeRewardChancePercent = InitialKeyChargeRewardChancePercent;

    private int _boxChancePercent = InitialBoxChancePercent;

    private int _lastKnownWingedBootsTimesUsed;

    private bool _hasSeenWingedBoots;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool IsUsedUp => ChargesRemaining <= 0;

    public override bool ShowCounter => true;

    public override int DisplayAmount => ChargesRemaining;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(RoomsKey, DefaultCharges)
    };

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/key.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/key.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/key.png";

    [SavedProperty]
    public int ChargesRemaining
    {
        get => _chargesRemaining;
        set
        {
            AssertMutable();
            _chargesRemaining = Math.Max(0, value);
            DynamicVars[RoomsKey].BaseValue = _chargesRemaining;
            Status = RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }
    }

    [SavedProperty]
    public int KeyChargeRewardChancePercent
    {
        get => _keyChargeRewardChancePercent;
        set
        {
            AssertMutable();
            _keyChargeRewardChancePercent = Math.Max(InitialKeyChargeRewardChancePercent, value);
        }
    }

    [SavedProperty]
    public int LastKnownWingedBootsTimesUsed
    {
        get => _lastKnownWingedBootsTimesUsed;
        set
        {
            AssertMutable();
            _lastKnownWingedBootsTimesUsed = value;
        }
    }

    [SavedProperty]
    public int BoxChancePercent
    {
        get => _boxChancePercent;
        set
        {
            AssertMutable();
            _boxChancePercent = Math.Max(InitialBoxChancePercent, value);
        }
    }

    [SavedProperty]
    public bool HasSeenWingedBoots
    {
        get => _hasSeenWingedBoots;
        set
        {
            AssertMutable();
            _hasSeenWingedBoots = value;
        }
    }

    public override bool ShouldAllowFreeTravel()
    {
        return !IsUsedUp && IsSinglePlayerRun() && !HasActiveWingedBoots();
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (IsUsedUp || !IsSinglePlayerRun())
        {
            return Task.CompletedTask;
        }

        if (Owner.RunState.CurrentRoomCount > 1)
        {
            return Task.CompletedTask;
        }

        if (Owner.RunState is not RunState runState)
        {
            return Task.CompletedTask;
        }

        if (runState.VisitedMapCoords.Count <= 1)
        {
            return Task.CompletedTask;
        }

        IReadOnlyList<MapCoord> visitedMapCoords = runState.VisitedMapCoords;
        MapCoord previousCoord = visitedMapCoords[visitedMapCoords.Count - 2];
        MapPoint previousPoint = runState.Map.GetPoint(previousCoord);
        if (previousPoint == null)
        {
            return Task.CompletedTask;
        }

        MapPoint currentMapPoint = Owner.RunState.CurrentMapPoint;
        if (currentMapPoint == null)
        {
            return Task.CompletedTask;
        }

        if (previousPoint.Children.Contains(currentMapPoint))
        {
            SyncWingedBootsUsageSnapshot();
            return Task.CompletedTask;
        }

        if (DidWingedBootsPayForThisTravel())
        {
            return Task.CompletedTask;
        }

        ChargesRemaining--;
        Flash();
        return Task.CompletedTask;
    }

    public void AddCharges(int amount = DefaultCharges)
    {
        if (amount <= 0)
        {
            return;
        }

        ChargesRemaining += amount;
        Flash();
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || room == null || !IsCombatRoom(room))
        {
            return false;
        }

        bool modified = false;

        if (player.PlayerRng.Rewards.NextInt(100) < KeyChargeRewardChancePercent)
        {
            var runState = Owner.RunState;
            var Kei = ModelDb.Card<KeyCharge>().ToMutable();
            runState.AddCard(Kei, player);
            rewards.Add(new SpecialCardReward(Kei, player));
            KeyChargeRewardChancePercent = InitialKeyChargeRewardChancePercent;
            Flash();
            modified = true;
        }
        else
        {
            KeyChargeRewardChancePercent += KeyChargeRewardChanceIncreasePercent;
        }

        if (player.PlayerRng.Rewards.NextInt(100) < BoxChancePercent)
        {
            rewards.Add(CreateDriftingCofferRewardSet(player));
            BoxChancePercent = InitialBoxChancePercent;
            Flash();
            modified = true;
        }
        else
        {
            BoxChancePercent += BoxChanceIncreasePercent;
        }

        return modified;
    }

    private static LinkedRewardSet CreateDriftingCofferRewardSet(Player player)
    {
        return new DriftingCofferLinkedRewardSet(new List<Reward>
        {
            new DriftingCofferChoiceReward(
                ModelDb.Relic<DriftingCoffer>().ToMutable(),
                player,
                OpenCofferWithKeyRewardDescriptionKey,
                requiresKeyCharge: true),
            new DriftingCofferChoiceReward(
                ModelDb.Relic<BrokenDriftingCoffer>().ToMutable(),
                player,
                BreakCofferWithHammerRewardDescriptionKey,
                requiresKeyCharge: false)
        }, player);
    }

    private bool IsSinglePlayerRun()
    {
        return Owner.RunState.Players.Count == 1;
    }

    private static bool IsCombatRoom(AbstractRoom room)
    {
        return room.RoomType is RoomType.Monster or RoomType.Elite or RoomType.Boss;
    }

    private bool HasActiveWingedBoots()
    {
        CoreWingedBoots? wingedBoots = Owner.GetRelic<CoreWingedBoots>();
        return wingedBoots != null && !wingedBoots.IsUsedUp;
    }

    private bool DidWingedBootsPayForThisTravel()
    {
        CoreWingedBoots? wingedBoots = Owner.GetRelic<CoreWingedBoots>();
        if (wingedBoots == null)
        {
            return false;
        }

        if (!HasSeenWingedBoots)
        {
            HasSeenWingedBoots = true;
            LastKnownWingedBootsTimesUsed = wingedBoots.TimesUsed;
            return !wingedBoots.IsUsedUp;
        }

        if (!wingedBoots.IsUsedUp || wingedBoots.TimesUsed > LastKnownWingedBootsTimesUsed)
        {
            LastKnownWingedBootsTimesUsed = wingedBoots.TimesUsed;
            return true;
        }

        LastKnownWingedBootsTimesUsed = wingedBoots.TimesUsed;
        return false;
    }

    private void SyncWingedBootsUsageSnapshot()
    {
        CoreWingedBoots? wingedBoots = Owner.GetRelic<CoreWingedBoots>();
        if (wingedBoots == null)
        {
            return;
        }

        HasSeenWingedBoots = true;
        LastKnownWingedBootsTimesUsed = wingedBoots.TimesUsed;
    }
}
