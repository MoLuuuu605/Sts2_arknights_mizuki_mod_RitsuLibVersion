using Arknights_Mizuki.Scripts.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Arknights_Mizuki.Scripts.StatusSlots;

public static class StatusSlotManager
{
    private static StatusSlotDataModifier? _hookModifier;
    private static RunState? _runState;

    public static void EnsureData(RunState runState)
    {
        _runState = runState;
        _hookModifier = GetOrCreateHookModifier(runState);
        StatusSlotRunData.EnsurePlayers(runState);
        _hookModifier.MigrateLegacyState(runState);
    }

    public static void AttachModifier(RunState runState)
    {
        if (_hookModifier == null)
            return;

        var existing = runState.Modifiers.OfType<StatusSlotDataModifier>().LastOrDefault();
        if (existing != null)
        {
            _hookModifier = existing;
            return;
        }

        runState.AddModifierDebug(_hookModifier);
        Entry.Logger.Info("[StatusSlot] Attached StatusSlotDataModifier hook carrier");
    }

    public static RunState? GetRunState() => _runState;

    internal static IEnumerable<Player> GetOrderedPlayers()
    {
        if (_runState == null)
            return Enumerable.Empty<Player>();

        return _runState.Players
            .Select(player => new { Player = player, Slot = _runState.GetPlayerSlotIndex(player) })
            .OrderBy(x => x.Slot < 0 ? int.MaxValue : x.Slot)
            .ThenBy(x => x.Player.NetId)
            .Select(x => x.Player);
    }

    internal static Player? GetLocalPlayer()
    {
        if (_runState == null || _runState.Players.Count == 0)
            return null;

        return LocalContext.GetMe(_runState) ??
               (_runState.Players.Count == 1 ? GetOrderedPlayers().FirstOrDefault() : null);
    }

    internal static Player? GetPlayerByNetId(ulong netId)
    {
        return _runState?.Players.FirstOrDefault(player => player.NetId == netId);
    }

    internal static Player? GetPlayerForCreature(Creature? creature)
    {
        if (_runState == null || creature == null)
            return null;

        foreach (Player player in _runState.Players)
        {
            if (ReferenceEquals(player.Creature, creature) ||
                ReferenceEquals(creature.PetOwner?.Creature, player.Creature))
            {
                return player;
            }
        }

        return null;
    }

    internal static bool IsMizukiPlayer(Player player) => player.Character is Mizuki;

    internal static bool IsPlayerEligibleForSlot(Player player, StatusSlotType type)
    {
        StatusSlotPlayerConfig config = StatusSlotRunData.GetConfig(player);
        bool enabled = type switch
        {
            StatusSlotType.Revelation => config.RevelationEnabled,
            StatusSlotType.Aberration => config.AberrationEnabled,
            StatusSlotType.SwarmCall => config.SwarmCallEnabled,
            _ => false
        };
        if (!enabled)
            return false;

        bool affectsOtherCharacters = type switch
        {
            StatusSlotType.Revelation => config.RevelationAffectsOtherCharacters,
            StatusSlotType.Aberration => config.AberrationAffectsOtherCharacters,
            StatusSlotType.SwarmCall => config.SwarmCallAffectsOtherCharacters,
            _ => false
        };
        return IsMizukiPlayer(player) || affectsOtherCharacters;
    }

    internal static bool ShouldSlotAffectCreature(StatusSlotType type, Creature creature)
    {
        Player? player = GetPlayerForCreature(creature);
        return player == null || IsPlayerEligibleForSlot(player, type);
    }

    internal static bool IsSlotEnabled(Player player, StatusSlotType type)
        => IsPlayerEligibleForSlot(player, type);

    internal static bool IsSlotEnabled(StatusSlotType type)
    {
        Player? localPlayer = GetLocalPlayer();
        return localPlayer != null && IsSlotEnabled(localPlayer, type);
    }

    internal static bool IsSlotVisibleForLocalPlayer(StatusSlotType type)
    {
        Player? localPlayer = GetLocalPlayer();
        return localPlayer != null && IsSlotEnabled(localPlayer, type);
    }

    public static StatusSlotPlayerState GetState(Player player) => StatusSlotRunData.GetState(player);

    internal static void CommitState(Player player, StatusSlotPlayerState state)
    {
        StatusSlotRunData.States.Set((RunState)player.RunState, player.NetId, state);
    }

    public static bool HasEffect(Player player, StatusSlotType type)
    {
        if (!IsSlotEnabled(player, type))
            return false;

        StatusSlotPlayerState state = GetState(player);
        return !string.IsNullOrEmpty(state.GetKey(type)) && state.GetCharges(type) > 0;
    }

    public static bool HasEffect(StatusSlotType type)
    {
        Player? localPlayer = GetLocalPlayer();
        return localPlayer != null && HasEffect(localPlayer, type);
    }

    public static string GetEffectKey(Player player, StatusSlotType type) => GetState(player).GetKey(type);
    public static int GetCharges(Player player, StatusSlotType type) => GetState(player).GetCharges(type);

    public static string GetEffectKey(StatusSlotType type)
        => GetLocalPlayer() is { } player ? GetEffectKey(player, type) : "";

    public static int GetCharges(StatusSlotType type)
        => GetLocalPlayer() is { } player ? GetCharges(player, type) : 0;

    public static void AssignEffect(Player player, StatusSlotType type, string effectKey)
        => AssignEffectAsync(player, type, effectKey, new ThrowingPlayerChoiceContext()).GetAwaiter().GetResult();

    public static async Task AssignEffectAsync(
        Player player,
        StatusSlotType type,
        string effectKey,
        PlayerChoiceContext? choiceContext = null)
    {
        var definition = StatusSlotEffects.FindByKey(effectKey);
        if (definition == null || definition.Slot != type)
            throw new ArgumentException($"Invalid effect '{effectKey}' for {type}.", nameof(effectKey));

        if (!IsSlotEnabled(player, type))
            return;

        StatusSlotPlayerState state = GetState(player);
        state.SetSlot(type, effectKey, definition.DefaultCharges);
        state.Revision++;
        CommitState(player, state);
        await HandleOnAcquiredAsync(player, type, effectKey, choiceContext ?? new ThrowingPlayerChoiceContext());
        Entry.Logger.Info($"[StatusSlot] Player {player.NetId} assigned {effectKey} ({type})");
        RefreshUI();
    }

    public static void RemoveEffect(Player player, StatusSlotType type)
        => RemoveEffectAsync(player, type, new ThrowingPlayerChoiceContext()).GetAwaiter().GetResult();

    public static async Task RemoveEffectAsync(
        Player player,
        StatusSlotType type,
        PlayerChoiceContext? choiceContext = null)
    {
        StatusSlotPlayerState state = GetState(player);
        string key = state.GetKey(type);
        if (string.IsNullOrEmpty(key) && state.GetCharges(type) <= 0)
            return;

        state.ClearSlot(type);
        state.Revision++;
        CommitState(player, state);
        await HandleOnRemovedAsync(player, type, key, choiceContext ?? new ThrowingPlayerChoiceContext());
        Entry.Logger.Info($"[StatusSlot] Player {player.NetId} removed {type}");
        RefreshUI();
    }

    internal static async Task DecrementAfterCombatAsync(
        Player player,
        StatusSlotType type,
        PlayerChoiceContext? choiceContext = null)
    {
        StatusSlotPlayerState state = GetState(player);
        int charges = state.GetCharges(type);
        string key = state.GetKey(type);
        if (charges <= 0 || string.IsNullOrEmpty(key))
            return;

        charges--;
        if (charges <= 0)
        {
            await RemoveEffectAsync(player, type, choiceContext);
            return;
        }

        state.SetSlot(type, key, charges);
        state.Revision++;
        CommitState(player, state);
    }

    internal static StatusSlotRoomAction BuildRoomAction(AbstractRoom room)
        => BuildRoomAction(room.RoomType);

    internal static StatusSlotRoomAction BuildRoomAction(RoomType roomType)
    {
        if (_runState == null)
            throw new InvalidOperationException("StatusSlot run data is not initialized.");

        var action = new StatusSlotRoomAction
        {
            ActIndex = _runState.CurrentActIndex,
            RoomCount = _runState.CurrentRoomCount,
            RoomType = (int)roomType
        };

        foreach (Player player in GetOrderedPlayers())
        {
            StatusSlotPlayerState state = GetState(player);
            StatusSlotPlayerConfig config = StatusSlotRunData.GetConfig(player);
            bool actChanged = state.LastActIndex != action.ActIndex;
            var playerAction = new StatusSlotRoomPlayerAction
            {
                PlayerNetId = player.NetId,
                ActChanged = actChanged
            };

            if (actChanged)
            {
                if (IsSlotEnabled(player, StatusSlotType.SwarmCall) &&
                    RollChance(player, $"swarm.roll.act.{action.ActIndex}", config.SwarmCallChance))
                {
                    playerAction.NewSwarmCallKey = PickEffect(
                        player,
                        StatusSlotType.SwarmCall,
                        $"swarm.pick.act.{action.ActIndex}",
                        randomOnly: true);
                }

                if (!HasEffect(player, StatusSlotType.Revelation) &&
                    IsSlotEnabled(player, StatusSlotType.Revelation) &&
                    RollChance(player, $"revelation.roll.act.{action.ActIndex}", config.RevelationChance))
                {
                    playerAction.NewRevelationKey = PickEffect(
                        player,
                        StatusSlotType.Revelation,
                        $"revelation.pick.act.{action.ActIndex}");
                }
            }

            action.Players.Add(playerAction);
        }

        return action;
    }

    internal static StatusSlotAberrationAction BuildAberrationAction()
    {
        if (_runState == null)
            throw new InvalidOperationException("StatusSlot run data is not initialized.");

        var action = new StatusSlotAberrationAction { RoomCount = _runState.CurrentRoomCount };
        foreach (Player player in GetOrderedPlayers())
        {
            StatusSlotPlayerState state = GetState(player);
            StatusSlotPlayerConfig config = StatusSlotRunData.GetConfig(player);
            if (state.LastAberrationCombatRoomCount >= action.RoomCount ||
                !IsSlotEnabled(player, StatusSlotType.Aberration) ||
                HasEffect(player, StatusSlotType.Aberration) ||
                state.DamageTakenThisCombat < config.AberrationHpThreshold ||
                !RollChance(player, $"aberration.roll.room.{action.RoomCount}", config.AberrationChance))
            {
                continue;
            }

            action.Players.Add(new StatusSlotAberrationPlayerAction
            {
                PlayerNetId = player.NetId,
                EffectKey = PickEffect(
                    player,
                    StatusSlotType.Aberration,
                    $"aberration.pick.room.{action.RoomCount}")
            });
        }

        return action;
    }

    internal static async Task ApplyRoomActionAsync(
        StatusSlotRoomAction action,
        PlayerChoiceContext choiceContext)
    {
        foreach (StatusSlotRoomPlayerAction playerAction in action.Players)
        {
            Player? player = GetPlayerByNetId(playerAction.PlayerNetId);
            if (player == null)
                continue;

            StatusSlotPlayerState state = GetState(player);
            if (state.LastProcessedRoomCount >= action.RoomCount)
                continue;

            if (playerAction.ActChanged && state.LastActIndex != action.ActIndex)
            {
                await RemoveSwarmCallForActChangeAsync(player, choiceContext);
                if (!string.IsNullOrEmpty(playerAction.NewSwarmCallKey))
                    await AssignEffectAsync(player, StatusSlotType.SwarmCall, playerAction.NewSwarmCallKey, choiceContext);
                if (!string.IsNullOrEmpty(playerAction.NewRevelationKey))
                    await AssignEffectAsync(player, StatusSlotType.Revelation, playerAction.NewRevelationKey, choiceContext);

                state = GetState(player);
                state.LastActIndex = action.ActIndex;
            }

            await ApplyRoomEnteredEffectsAsync(player, (RoomType)action.RoomType, choiceContext);
            state = GetState(player);
            state.LastProcessedRoomCount = action.RoomCount;
            state.Revision++;
            CommitState(player, state);
            Entry.Logger.Info(
                $"[StatusSlot][Sync] room={action.RoomCount} player={player.NetId} " +
                $"rev={state.RevelationKey}/{state.RevelationCharges} " +
                $"aber={state.AberrationKey}/{state.AberrationCharges} " +
                $"swarm={state.SwarmCallKey}/{state.SwarmCallCharges} revision={state.Revision}");
        }

        RefreshUI();
    }

    internal static async Task ApplyAberrationActionAsync(
        StatusSlotAberrationAction action,
        PlayerChoiceContext choiceContext)
    {
        var effectByPlayer = action.Players.ToDictionary(x => x.PlayerNetId);
        foreach (Player player in GetOrderedPlayers())
        {
            StatusSlotPlayerState state = GetState(player);
            if (state.LastAberrationCombatRoomCount >= action.RoomCount)
                continue;

            state.LastAberrationCombatRoomCount = action.RoomCount;
            state.Revision++;
            CommitState(player, state);

            if (effectByPlayer.TryGetValue(player.NetId, out var playerAction) &&
                !string.IsNullOrEmpty(playerAction.EffectKey))
            {
                await AssignEffectAsync(
                    player,
                    StatusSlotType.Aberration,
                    playerAction.EffectKey,
                    choiceContext);
            }

            Entry.Logger.Info(
                $"[StatusSlot][Sync] combatRoom={action.RoomCount} player={player.NetId} " +
                $"aberration={GetState(player).AberrationKey} revision={GetState(player).Revision}");
        }

        RefreshUI();
    }

    public static void RefreshUI()
    {
        Player? player = GetLocalPlayer();
        if (player == null)
            return;

        StatusSlotPlayerState state = GetState(player);
        StatusSlotPlayerConfig config = StatusSlotRunData.GetConfig(player);
        for (int i = 0; i < 3; i++)
        {
            var type = (StatusSlotType)i;
            bool visible = IsSlotEnabled(player, type);
            StatusSlotFrame.SetSlotVisible(i, visible);
            string key = state.GetKey(type);
            int charges = state.GetCharges(type);

            if (!visible || string.IsNullOrEmpty(key) || charges <= 0)
            {
                string emptyDescription = type == StatusSlotType.Aberration
                    ? $"单场战斗失去 {config.AberrationHpThreshold} 点以上生命值时，有 {config.AberrationChance / 100}% 概率获得一个随机的排异反应。"
                    : StatusSlotI18n.GetSlotEmpty(type);
                StatusSlotFrame.SetContent(
                    i,
                    (string?)null,
                    StatusSlotI18n.GetSlotName(type),
                    emptyDescription);
                continue;
            }

            var definition = StatusSlotEffects.FindByKey(key);
            if (definition == null)
                continue;

            string description = StatusSlotI18n.GetEffectDesc(key);
            if (type != StatusSlotType.SwarmCall && charges < 999)
            {
                description += charges == 1
                    ? " \n（在本场战斗后清除）"
                    : $" \n（{charges} 场战斗后移除）";
            }
            StatusSlotFrame.SetContent(
                i,
                definition.IconPath,
                StatusSlotI18n.GetEffectName(key),
                description);
        }
    }

    private static async Task RemoveSwarmCallForActChangeAsync(
        Player player,
        PlayerChoiceContext choiceContext)
    {
        StatusSlotPlayerState state = GetState(player);
        if (state.SwarmCallKey == "echo_modification")
        {
            int goldToRemove = Math.Min(player.Gold, Math.Max(0, state.EchoModificationGoldGained));
            if (goldToRemove > 0)
                await PlayerCmd.LoseGold(goldToRemove, player, GoldLossType.Spent);
        }

        await RemoveEffectAsync(player, StatusSlotType.SwarmCall, choiceContext);
    }

    private static async Task ApplyRoomEnteredEffectsAsync(
        Player player,
        RoomType roomType,
        PlayerChoiceContext choiceContext)
    {
        if (HasEffect(player, StatusSlotType.SwarmCall) &&
            GetEffectKey(player, StatusSlotType.SwarmCall) == "echo_modification" &&
            !IsCombatRoom(roomType))
        {
            await PlayerCmd.GainGold(25m, player);
        }

        if (HasEffect(player, StatusSlotType.SwarmCall) &&
            GetEffectKey(player, StatusSlotType.SwarmCall) == "echo_wither" &&
            IsCombatRoom(roomType))
        {
            await CreatureCmd.Damage(
                choiceContext,
                player.Creature,
                2,
                ValueProp.Unblockable,
                null,
                null);
        }
    }

    private static async Task HandleOnAcquiredAsync(
        Player player,
        StatusSlotType type,
        string effectKey,
        PlayerChoiceContext choiceContext)
    {
        StatusSlotPlayerState state = GetState(player);
        switch (effectKey)
        {
            case "sargon_generosity":
                await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(player).ToMutable(), player);
                await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(player).ToMutable(), player);
                break;
            case "flesh_distortion":
                state.FleshDistortionLostHp = player.Creature.MaxHp / 2;
                CommitState(player, state);
                await CreatureCmd.LoseMaxHp(
                    choiceContext,
                    player.Creature,
                    state.FleshDistortionLostHp,
                    isFromCard: false);
                break;
            case "echo_modification":
                state.EchoModificationGoldOnAcquire = player.Gold;
                state.EchoModificationGoldGained = 0;
                CommitState(player, state);
                break;
            case "columbia_innovation":
                state.ColumbiaPendingReward = true;
                CommitState(player, state);
                break;
        }
    }

    private static async Task HandleOnRemovedAsync(
        Player player,
        StatusSlotType type,
        string effectKey,
        PlayerChoiceContext choiceContext)
    {
        if (effectKey != "flesh_distortion")
            return;

        StatusSlotPlayerState state = GetState(player);
        int lostHp = state.FleshDistortionLostHp;
        if (lostHp <= 0)
            return;

        state.FleshDistortionLostHp = 0;
        CommitState(player, state);
        await CreatureCmd.GainMaxHp(player.Creature, lostHp);
    }

    private static bool RollChance(Player player, string streamId, int chanceBasisPoints)
    {
        if (chanceBasisPoints <= 0)
            return false;
        if (chanceBasisPoints >= 10000)
            return true;
        return AuthorityRandom(player, streamId, 10000) < chanceBasisPoints;
    }

    private static string PickEffect(
        Player player,
        StatusSlotType type,
        string streamId,
        bool randomOnly = false)
    {
        var effects = StatusSlotEffects.GetForSlot(type);
        if (randomOnly)
            effects = effects.Where(effect => effect.IsRandomObtainable).ToList();
        if (effects.Count == 0)
            return "";
        return effects[AuthorityRandom(player, streamId, effects.Count)].Key;
    }

    private static int AuthorityRandom(Player player, string streamId, int exclusiveMax)
    {
        if (exclusiveMax <= 0)
            throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

        RunState runState = _runState ?? (RunState)player.RunState;
        string runSeed = runState.Rng.StringSeed;
        if (string.IsNullOrEmpty(runSeed))
            runSeed = runState.Rng.Seed.ToString("X16");

        int slotIndex = runState.GetPlayerSlotIndex(player);
        byte[] input = Encoding.UTF8.GetBytes($"{runSeed}|{slotIndex}|{Entry.ModId}|{streamId}");
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(input, digest);
        return (int)(BinaryPrimitives.ReadUInt32LittleEndian(digest) % (uint)exclusiveMax);
    }

    private static bool IsCombatRoom(RoomType roomType)
        => roomType is RoomType.Monster or RoomType.Elite or RoomType.Boss;

    private static StatusSlotDataModifier GetOrCreateHookModifier(RunState runState)
    {
        var existing = runState.Modifiers.OfType<StatusSlotDataModifier>().LastOrDefault();
        if (existing != null)
            return existing;

        var modifier = (StatusSlotDataModifier)ModelDb.Modifier<StatusSlotDataModifier>().ToMutable();
        modifier.OnRunLoaded(runState);
        return modifier;
    }
}
