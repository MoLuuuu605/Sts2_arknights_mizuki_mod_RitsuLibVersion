using System.Reflection;
using System.Threading;
using Arknights_Mizuki.Scripts.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(NLinkedRewardSet), "GetReward")]
public static class LinkedRewardSetGetRewardPatch
{
    private static readonly FieldInfo? RewardsScreenField = AccessTools.Field(typeof(NLinkedRewardSet), "_rewardsScreen");
    private static readonly FieldInfo? RewardSuccessfullySelectedField = AccessTools.Field(typeof(Reward), "<SuccessfullySelected>k__BackingField");

    private static bool Prefix(NLinkedRewardSet __instance)
    {
        CompleteSelection(__instance);
        return false;
    }

    internal static void CompleteSelection(NLinkedRewardSet linkedRewardSet)
    {
        if (linkedRewardSet.IsQueuedForDeletion())
        {
            return;
        }

        MarkSuccessfullySelected(linkedRewardSet.LinkedRewardSet);

        if (RewardsScreenField?.GetValue(linkedRewardSet) is NRewardsScreen rewardsScreen)
        {
            rewardsScreen.RewardCollectedFrom(linkedRewardSet);
        }

        linkedRewardSet.LinkedRewardSet.OnSkipped();
        if (!linkedRewardSet.IsQueuedForDeletion())
        {
            linkedRewardSet.QueueFree();
        }
    }

    internal static void MarkSuccessfullySelected(Reward reward)
    {
        RewardSuccessfullySelectedField?.SetValue(reward, true);
    }
}

[HarmonyPatch(typeof(NRewardButton), "Reload")]
public static class DriftingCofferChoiceRewardButtonReloadPatch
{
    private static void Postfix(NRewardButton __instance)
    {
        DriftingCofferChoiceRewardButtonState.DisableIfBlocked(__instance);
    }
}

[HarmonyPatch(typeof(NRewardButton), "GetReward")]
public static class LinkedRewardButtonGetRewardPatch
{
    private static bool Prefix(NRewardButton __instance, ref Task __result)
    {
        Reward reward = __instance.Reward;
        if (DriftingCofferChoiceRewardButtonState.IsBlocked(reward))
        {
            __instance.Disable();
            __result = Task.CompletedTask;
            return false;
        }

        if (reward.ParentRewardSet == null && !IsDriftingCofferReward(reward))
        {
            return true;
        }

        __result = GetReward(__instance);
        return false;
    }

    private static async Task GetReward(NRewardButton rewardButton)
    {
        Reward reward = rewardButton.Reward;
        NRewardsScreen? rewardsScreen = RewardScreenExtraRewardInjector.FindRewardsScreen(rewardButton);
        using RewardScreenExtraRewardInjector.SelectionScope? scope = RewardScreenExtraRewardInjector.BeginSelection(rewardsScreen);

        rewardButton.Disable();
        bool success = await RunManager.Instance.RewardsSetSynchronizer.SelectLocalReward(reward);
        if (!success)
        {
            rewardButton.Enable();
            return;
        }

        if (reward.ParentRewardSet != null)
        {
            LinkedRewardSetGetRewardPatch.MarkSuccessfullySelected(reward.ParentRewardSet);
            if (rewardsScreen != null)
            {
                RewardScreenExtraRewardInjector.FlushPendingRewards(rewardsScreen);
            }

            NLinkedRewardSet? linkedRewardSet = FindLinkedRewardSet(rewardButton);
            if (linkedRewardSet != null && !linkedRewardSet.IsQueuedForDeletion())
            {
                LinkedRewardSetGetRewardPatch.CompleteSelection(linkedRewardSet);
            }

            return;
        }

        if (rewardsScreen != null)
        {
            RewardScreenExtraRewardInjector.FlushPendingRewards(rewardsScreen);
            rewardsScreen.RewardCollectedFrom(rewardButton);
        }
        else if (!rewardButton.IsQueuedForDeletion())
        {
            rewardButton.QueueFree();
        }
    }

    private static bool IsDriftingCofferReward(Reward reward)
    {
        return reward is RelicReward relicReward
            && (relicReward.Relic is DriftingCoffer || relicReward.ClaimedRelic is DriftingCoffer);
    }

    private static NLinkedRewardSet? FindLinkedRewardSet(Node node)
    {
        Node? current = node;
        while (current != null)
        {
            if (current is NLinkedRewardSet linkedRewardSet)
            {
                return linkedRewardSet;
            }

            current = current.GetParent();
        }

        return null;
    }
}

internal static class DriftingCofferChoiceRewardButtonState
{
    public static bool IsBlocked(Reward reward)
    {
        return reward is DriftingCofferChoiceReward choiceReward && !choiceReward.IsSelectable;
    }

    public static void DisableIfBlocked(NRewardButton rewardButton)
    {
        if (IsBlocked(rewardButton.Reward))
        {
            rewardButton.Disable();
        }
    }
}

internal static class RewardScreenExtraRewardInjector
{
    internal sealed class SelectionContext
    {
        public SelectionContext(NRewardsScreen rewardsScreen)
        {
            RewardsScreen = rewardsScreen;
        }

        public NRewardsScreen RewardsScreen { get; }

        public List<Reward> PendingRewards { get; } = new();
    }

    internal sealed class SelectionScope : IDisposable
    {
        private readonly SelectionContext? _previous;

        internal SelectionScope(SelectionContext context)
        {
            _previous = Current.Value;
            Current.Value = context;
        }

        public void Dispose()
        {
            Current.Value = _previous;
        }
    }

    private static readonly AsyncLocal<SelectionContext?> Current = new();
    private static readonly FieldInfo? RewardsSetField = AccessTools.Field(typeof(NRewardsScreen), "_rewardsSet");
    private static readonly FieldInfo? RewardsContainerField = AccessTools.Field(typeof(NRewardsScreen), "_rewardsContainer");
    private static readonly FieldInfo? RewardButtonsField = AccessTools.Field(typeof(NRewardsScreen), "_rewardButtons");
    private static readonly MethodInfo? UpdateScreenStateMethod = AccessTools.Method(typeof(NRewardsScreen), "UpdateScreenState");

    public static SelectionScope? BeginSelection(NRewardsScreen? rewardsScreen)
    {
        return rewardsScreen == null ? null : new SelectionScope(new SelectionContext(rewardsScreen));
    }

    public static bool TryAppendToCurrentSelection(Player player, List<Reward> rewards)
    {
        SelectionContext? context = Current.Value;
        if (context == null || rewards.Count == 0)
        {
            return false;
        }

        if (RewardsSetField?.GetValue(context.RewardsScreen) is not RewardsSet rewardsSet || rewardsSet.Player != player)
        {
            return false;
        }

        foreach (Reward reward in rewards)
        {
            reward.Populate();
            reward.MarkContentAsSeen();
            rewardsSet.Rewards.Add(reward);
            context.PendingRewards.Add(reward);
        }

        return true;
    }

    public static void FlushPendingRewards(NRewardsScreen rewardsScreen)
    {
        SelectionContext? context = Current.Value;
        if (context == null || context.RewardsScreen != rewardsScreen || context.PendingRewards.Count == 0)
        {
            return;
        }

        if (RewardsContainerField?.GetValue(rewardsScreen) is not Control rewardsContainer
            || RewardButtonsField?.GetValue(rewardsScreen) is not List<Control> rewardButtons)
        {
            return;
        }

        foreach (Reward reward in context.PendingRewards)
        {
            NRewardButton button = NRewardButton.Create(reward, rewardsScreen);
            button.Connect(NRewardButton.SignalName.RewardClaimed, Callable.From<NRewardButton>(b => rewardsScreen.RewardCollectedFrom(b)));
            button.Connect(NRewardButton.SignalName.RewardSkipped, Callable.From<NRewardButton>(b => rewardsScreen.RewardSkippedFrom(b)));
            rewardButtons.Add(button);
            rewardsContainer.AddChild(button);
        }

        context.PendingRewards.Clear();
        UpdateScreenStateMethod?.Invoke(rewardsScreen, null);
    }

    public static NRewardsScreen? FindRewardsScreen(Node node)
    {
        Node? current = node;
        while (current != null)
        {
            if (current is NRewardsScreen rewardsScreen)
            {
                return rewardsScreen;
            }

            current = current.GetParent();
        }

        return null;
    }
}
