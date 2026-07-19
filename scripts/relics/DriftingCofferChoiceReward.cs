using Arknights_Mizuki.Scripts.Patches;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Relics;

internal sealed class DriftingCofferChoiceReward : RelicReward
{
    private readonly string _descriptionKey;

    public DriftingCofferChoiceReward(RelicModel relic, Player player, string descriptionKey, bool requiresKeyCharge)
        : base(relic, player)
    {
        _descriptionKey = descriptionKey;
        RequiresKeyCharge = requiresKeyCharge;
    }

    public bool RequiresKeyCharge { get; }

    public bool IsSelectable => !RequiresKeyCharge || Player.GetRelic<Key>()?.ChargesRemaining > 0;

    public override LocString Description => new("relics", _descriptionKey);

    protected override async Task<bool> OnSelect()
    {
        if (!IsSelectable)
            return false;

        return await base.OnSelect();
    }
}

internal sealed class DriftingCofferLinkedRewardSet : LinkedRewardSet
{
    private int? _localChoiceIndex;

    internal DriftingCofferLinkedRewardSet(List<Reward> rewards, Player player)
        : base(rewards, player)
    {
    }

    internal void SetLocalChoice(DriftingCofferChoiceReward reward)
    {
        int choiceIndex = -1;
        for (int i = 0; i < Rewards.Count; i++)
        {
            if (ReferenceEquals(Rewards[i], reward))
            {
                choiceIndex = i;
                break;
            }
        }

        if (choiceIndex < 0)
            throw new InvalidOperationException("Drifting coffer choice is not part of its linked reward set.");

        _localChoiceIndex = choiceIndex;
    }

    protected override async Task<bool> OnSelect()
    {
        var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
        uint choiceId = synchronizer.ReserveChoiceId(Player);
        int choiceIndex;

        if (Player.NetId == RunManager.Instance.NetService.NetId)
        {
            choiceIndex = _localChoiceIndex ??
                throw new InvalidOperationException("No local drifting coffer choice was recorded.");
            synchronizer.SyncLocalChoice(Player, choiceId, PlayerChoiceResult.FromIndex(choiceIndex));
        }
        else
        {
            choiceIndex = (await synchronizer.WaitForRemoteChoice(Player, choiceId)).AsIndex();
        }

        if (choiceIndex < 0 || choiceIndex >= Rewards.Count)
            throw new InvalidOperationException($"Invalid drifting coffer choice index {choiceIndex}.");

        Reward selectedReward = Rewards[choiceIndex];
        LinkedRewardSet? originalParent = selectedReward.ParentRewardSet;
        selectedReward.ParentRewardSet = null;
        try
        {
            bool selected = await selectedReward.SelectUnsynchronized();
            if (!selected)
                return false;

            if (selectedReward is DriftingCofferChoiceReward choiceReward &&
                choiceReward.Relic is DriftingCoffer)
            {
                RelicRewardAnimationState.ClearClaimedRelic(choiceReward);
            }

            foreach (Reward reward in Rewards.ToArray())
            {
                if (!ReferenceEquals(reward, selectedReward))
                    reward.OnSkipped();
                RemoveReward(reward);
            }

            Entry.Logger.Info(
                $"[DriftingCoffer] Selected linked choice index={choiceIndex} player={Player.NetId} " +
                $"local={Player.NetId == RunManager.Instance.NetService.NetId}");
            return true;
        }
        finally
        {
            selectedReward.ParentRewardSet = originalParent;
            _localChoiceIndex = null;
        }
    }
}
