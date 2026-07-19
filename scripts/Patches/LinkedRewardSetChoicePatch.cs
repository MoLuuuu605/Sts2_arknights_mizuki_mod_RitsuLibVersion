using System.Reflection;
using System.Threading;
using Arknights_Mizuki.Scripts.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rewards;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(NRewardButton), "Reload")]
public static class DriftingCofferChoiceRewardButtonReloadPatch
{
    private static void Postfix(NRewardButton __instance)
    {
        DriftingCofferChoiceRewardButtonState.DisableIfBlocked(__instance);
    }
}

[HarmonyPatch(typeof(NRewardButton), "GetReward")]
public static class DriftingCofferChoiceRewardButtonGetRewardPatch
{
    private static bool Prefix(
        NRewardButton __instance,
        ref Task __result,
        out NLinkedRewardSet? __state)
    {
        __state = __instance.Reward is DriftingCofferChoiceReward
            ? FindLinkedRewardSet(__instance)
            : null;
        RewardScreenExtraRewardInjector.RegisterLocalScreen(__instance);

        if (!DriftingCofferChoiceRewardButtonState.IsBlocked(__instance.Reward))
            return true;

        __instance.Disable();
        __result = Task.CompletedTask;
        return false;
    }

    private static void Postfix(ref Task __result, NLinkedRewardSet? __state)
    {
        if (__state != null)
            __result = RemoveLinkedUiAfterSelection(__result, __state);
    }

    private static async Task RemoveLinkedUiAfterSelection(
        Task selectionTask,
        NLinkedRewardSet linkedRewardSet)
    {
        try
        {
            await selectionTask;
        }
        finally
        {
            if (linkedRewardSet.LinkedRewardSet.SuccessfullySelected &&
                !linkedRewardSet.IsQueuedForDeletion())
            {
                NRewardsScreen? rewardsScreen = RewardScreenExtraRewardInjector.FindRewardsScreen(linkedRewardSet);
                if (rewardsScreen != null)
                    rewardsScreen.RewardCollectedFrom(linkedRewardSet);
                else
                    linkedRewardSet.QueueFree();

                Entry.Logger.Info("[DriftingCoffer] Removed linked reward UI after selection");
            }
        }
    }

    private static NLinkedRewardSet? FindLinkedRewardSet(Node node)
    {
        Node? current = node;
        while (current != null)
        {
            if (current is NLinkedRewardSet linkedRewardSet)
                return linkedRewardSet;
            current = current.GetParent();
        }

        return null;
    }
}

[HarmonyPatch]
public static class DriftingCofferSynchronizedRewardSelectionPatch
{
    private static readonly FieldInfo? RewardsSetStateSetField = AccessTools.Field(
        typeof(RewardsSetSynchronizer).GetNestedType(
            "RewardsSetState",
            BindingFlags.Public | BindingFlags.NonPublic),
        "set");

    private static MethodBase TargetMethod()
    {
        return AccessTools.GetDeclaredMethods(typeof(RewardsSetSynchronizer))
            .Single(method =>
            {
                if (method.Name != "SelectRewardForPlayer" || method.ReturnType != typeof(Task<bool>))
                    return false;

                ParameterInfo[] parameters = method.GetParameters();
                return parameters.Length == 2 && parameters[1].ParameterType == typeof(Reward);
            });
    }

    private static void Prefix(
        object __0,
        Reward __1,
        out RewardScreenExtraRewardInjector.SelectionScope? __state)
    {
        __state = null;
        if (__1 is not DriftingCofferLinkedRewardSet ||
            RewardsSetStateSetField?.GetValue(__0) is not RewardsSet rewardsSet)
        {
            return;
        }

        __state = RewardScreenExtraRewardInjector.BeginSelection(rewardsSet);
    }

    private static void Postfix(
        ref Task<bool> __result,
        RewardScreenExtraRewardInjector.SelectionScope? __state)
    {
        if (__state != null)
            __result = __state.CompleteAfter(__result);
    }
}

[HarmonyPatch(typeof(RewardsSetSynchronizer), nameof(RewardsSetSynchronizer.SelectLocalReward))]
public static class DriftingCofferLocalLinkedRewardSelectionPatch
{
    private static void Prefix(ref Reward __0)
    {
        if (__0 is not DriftingCofferChoiceReward choiceReward ||
            choiceReward.ParentRewardSet is not DriftingCofferLinkedRewardSet linkedRewardSet)
        {
            return;
        }

        linkedRewardSet.SetLocalChoice(choiceReward);
        __0 = linkedRewardSet;
    }
}

internal static class DriftingCofferChoiceRewardButtonState
{
    internal static bool IsBlocked(Reward reward)
        => reward is DriftingCofferChoiceReward choiceReward && !choiceReward.IsSelectable;

    internal static void DisableIfBlocked(NRewardButton rewardButton)
    {
        if (IsBlocked(rewardButton.Reward))
            rewardButton.Disable();
    }
}

internal static class RelicRewardAnimationState
{
    private static readonly FieldInfo? ClaimedRelicField =
        AccessTools.Field(typeof(RelicReward), "<ClaimedRelic>k__BackingField");

    internal static void ClearClaimedRelic(RelicReward reward)
    {
        ClaimedRelicField?.SetValue(reward, null);
    }
}

internal static class RewardScreenExtraRewardInjector
{
    internal sealed class SelectionContext
    {
        internal SelectionContext(RewardsSet rewardsSet, NRewardsScreen? rewardsScreen)
        {
            RewardsSet = rewardsSet;
            RewardsScreen = rewardsScreen;
        }

        internal RewardsSet RewardsSet { get; }
        internal NRewardsScreen? RewardsScreen { get; }
        internal List<Reward> PendingRewards { get; } = new();
    }

    internal sealed class SelectionScope : IDisposable
    {
        private readonly SelectionContext? _previous;
        private bool _disposed;

        internal SelectionScope(SelectionContext context)
        {
            Context = context;
            _previous = Current.Value;
            Current.Value = context;
        }

        internal SelectionContext Context { get; }

        internal async Task<bool> CompleteAfter(Task<bool> selectionTask)
        {
            try
            {
                bool selected = await selectionTask;
                if (!selected)
                {
                    RollbackPendingRewards(Context);
                }
                else if (Context.RewardsScreen != null)
                {
                    FlushPendingRewards(Context);
                }

                return selected;
            }
            catch
            {
                RollbackPendingRewards(Context);
                throw;
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Current.Value = _previous;
        }
    }

    private static readonly AsyncLocal<SelectionContext?> Current = new();
    private static readonly Dictionary<ulong, WeakReference<NRewardsScreen>> LocalScreens = new();
    private static readonly FieldInfo? RewardsSetField = AccessTools.Field(typeof(NRewardsScreen), "_rewardsSet");
    private static readonly FieldInfo? RewardsContainerField = AccessTools.Field(typeof(NRewardsScreen), "_rewardsContainer");
    private static readonly FieldInfo? RewardButtonsField = AccessTools.Field(typeof(NRewardsScreen), "_rewardButtons");
    private static readonly MethodInfo? UpdateScreenStateMethod = AccessTools.Method(typeof(NRewardsScreen), "UpdateScreenState");

    internal static void RegisterLocalScreen(NRewardButton rewardButton)
    {
        NRewardsScreen? rewardsScreen = FindRewardsScreen(rewardButton);
        if (rewardsScreen != null)
            LocalScreens[rewardButton.Reward.Player.NetId] = new WeakReference<NRewardsScreen>(rewardsScreen);
    }

    internal static SelectionScope BeginSelection(RewardsSet rewardsSet)
    {
        NRewardsScreen? rewardsScreen = null;
        if (LocalScreens.TryGetValue(rewardsSet.Player.NetId, out WeakReference<NRewardsScreen>? reference) &&
            reference.TryGetTarget(out NRewardsScreen? candidate) &&
            !candidate.IsQueuedForDeletion() &&
            ReferenceEquals(RewardsSetField?.GetValue(candidate), rewardsSet))
        {
            rewardsScreen = candidate;
        }

        return new SelectionScope(new SelectionContext(rewardsSet, rewardsScreen));
    }

    internal static bool TryAppendToCurrentSelection(Player player, List<Reward> rewards)
    {
        SelectionContext? context = Current.Value;
        if (context == null || rewards.Count == 0 || !ReferenceEquals(context.RewardsSet.Player, player))
            return false;

        foreach (Reward reward in rewards)
        {
            reward.Populate();
            reward.MarkContentAsSeen();
            context.RewardsSet.Rewards.Add(reward);
            context.PendingRewards.Add(reward);
        }

        Entry.Logger.Info(
            $"[DriftingCoffer] Appended {rewards.Count} rewards to set={context.RewardsSet.Id} " +
            $"player={player.NetId} localUi={context.RewardsScreen != null}");

        return true;
    }

    private static void FlushPendingRewards(SelectionContext context)
    {
        NRewardsScreen rewardsScreen = context.RewardsScreen!;
        if (context.PendingRewards.Count == 0 ||
            RewardsContainerField?.GetValue(rewardsScreen) is not Control rewardsContainer ||
            RewardButtonsField?.GetValue(rewardsScreen) is not List<Control> rewardButtons)
        {
            return;
        }

        foreach (Reward reward in context.PendingRewards)
        {
            NRewardButton button = NRewardButton.Create(reward, rewardsScreen);
            button.Connect(
                NRewardButton.SignalName.RewardClaimed,
                Callable.From<NRewardButton>(rewardsScreen.RewardCollectedFrom));
            button.Connect(
                NRewardButton.SignalName.RewardSkipped,
                Callable.From<NRewardButton>(rewardsScreen.RewardSkippedFrom));
            rewardButtons.Add(button);
            rewardsContainer.AddChild(button);
        }

        context.PendingRewards.Clear();
        UpdateScreenStateMethod?.Invoke(rewardsScreen, null);
    }

    private static void RollbackPendingRewards(SelectionContext context)
    {
        foreach (Reward reward in context.PendingRewards)
            context.RewardsSet.Rewards.Remove(reward);

        context.PendingRewards.Clear();
    }

    internal static NRewardsScreen? FindRewardsScreen(Node node)
    {
        Node? current = node;
        while (current != null)
        {
            if (current is NRewardsScreen rewardsScreen)
                return rewardsScreen;
            current = current.GetParent();
        }

        return null;
    }
}
