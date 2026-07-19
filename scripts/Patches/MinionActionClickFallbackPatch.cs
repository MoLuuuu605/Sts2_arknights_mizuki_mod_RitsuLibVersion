using Arknights_Mizuki.Scripts.Actions;
using Arknights_Mizuki.Scripts.Minions;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MinionLib.Action;
using MinionLib.Action.GameActions;
using MinionLib.Targeting;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class MinionActionClickFallbackPatch
{
    private const string Module = "[MinionActionFallback]";
    private static readonly MethodInfo? MinionLibTryEnqueueMethod = typeof(ActionModel).Assembly
        .GetType("MinionLib.Action.CreatureActionQueueService")
        ?.GetMethod("TryEnqueue", BindingFlags.Public | BindingFlags.Static);

    private static readonly HashSet<(uint ActorCombatId, string ActionEntry)> PendingActions = [];

    [HarmonyPostfix]
    private static void Postfix(NCreature __instance)
    {
        if (!IsSupportedMinion(__instance))
            return;

        __instance.Hitbox.MouseFilter = Control.MouseFilterEnum.Stop;
        var connected = ConnectInputControls(__instance);

        Entry.Logger.Info(
            $"{Module} connected actor={DescribeCreature(__instance)} controls={connected} hitboxFilter={__instance.Hitbox.MouseFilter} hitboxPos={__instance.Hitbox.GlobalPosition} hitboxSize={__instance.Hitbox.Size}");
    }

    private static void OnHitboxGuiInput(NCreature actorNode, InputEvent inputEvent)
    {
        if (NTargetManager.Instance.IsInSelection)
            return;

        var triggeredByMouse =
            inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouseButton &&
            mouseButton.IsReleased();

        var triggeredByController =
            inputEvent is InputEventAction { Action: var action } actionEvent &&
            action == MegaInput.select &&
            actionEvent.IsPressed() &&
            actorNode.Hitbox.HasFocus();

        if (!triggeredByMouse && !triggeredByController)
            return;

        if (!IsSupportedMinion(actorNode))
            return;

        var actor = actorNode.Entity;
        var actionPower = actor.Powers
            .OfType<ActionModel>()
            .FirstOrDefault(IsSupportedAction);

        Entry.Logger.Info(
            $"{Module} input actor={DescribeCreature(actorNode)} action={DescribeAction(actionPower)} actions={DescribeActions(actor)}");

        if (actionPower == null)
            return;

        if (actor.PetOwner != null && !LocalContext.IsMe(actor.PetOwner))
        {
            Entry.Logger.Info($"{Module} ignored non-local pet owner actor={DescribeCreature(actorNode)}");
            return;
        }

        TaskHelper.RunSafely(TryUseActionAsync(actorNode, actionPower, triggeredByController));
        actorNode.GetViewport().SetInputAsHandled();
    }

    internal static async Task TryUseActionAsync(NCreature actorNode, ActionModel requestedAction, bool useController)
    {
        var actor = actorNode.Entity;
        if (actor.CombatId == null)
        {
            Entry.Logger.Info($"{Module} rejected actor without combat id actor={DescribeCreature(actorNode)}");
            return;
        }

        var key = (actor.CombatId.Value, requestedAction.Id.Entry);
        lock (PendingActions)
        {
            if (!PendingActions.Add(key))
            {
                Entry.Logger.Info($"{Module} ignored duplicate pending click actor={DescribeCreature(actorNode)} action={requestedAction.Id.Entry}");
                return;
            }
        }

        try
        {
            var ready = await WaitUntilReadyAsync(actorNode, requestedAction);
            if (!ready)
                return;

            var actionPower = FindMatchingAction(actorNode.Entity, requestedAction);
            if (actionPower == null)
            {
                Entry.Logger.Info($"{Module} rejected missing action after wait actor={DescribeCreature(actorNode)} action={requestedAction.Id.Entry}");
                return;
            }

            var combatState = actorNode.Entity.CombatState;
            if (combatState == null)
            {
                Entry.Logger.Info($"{Module} rejected null combat state actor={DescribeCreature(actorNode)}");
                return;
            }

            var targetType = actionPower.TargetType;
            var singleTarget = IsSingleTarget(targetType);
            var validTargets = actionPower.GetValidTargets(combatState);

            Entry.Logger.Info(
                $"{Module} ready actor={DescribeCreature(actorNode)} action={DescribeAction(actionPower)} queue={RunManager.Instance.ActionQueueSynchronizer.CombatState} playerDisabled={CombatManager.Instance.PlayerActionsDisabled} side={actorNode.Entity.Side} currentSide={combatState.CurrentSide} targetType={targetType} single={singleTarget} validTargets={validTargets.Count}");

            if (targetType == TargetType.None)
            {
                TryEnqueue(actionPower, null);
                return;
            }

            if (!singleTarget)
            {
                if (validTargets.Count == 0)
                {
                    Entry.Logger.Info($"{Module} rejected no valid multi-targets actor={DescribeCreature(actorNode)} action={DescribeAction(actionPower)}");
                    return;
                }

                TryEnqueue(actionPower, null);
                return;
            }

            if (targetType == TargetType.Self)
            {
                TryEnqueue(actionPower, null);
                return;
            }

            if (validTargets.Count == 0)
            {
                Entry.Logger.Info($"{Module} rejected no valid single-targets actor={DescribeCreature(actorNode)} action={DescribeAction(actionPower)}");
                return;
            }

            await TargetAndEnqueueAsync(actorNode, actionPower, useController);
        }
        finally
        {
            lock (PendingActions)
                PendingActions.Remove(key);
        }
    }

    private static async Task<bool> WaitUntilReadyAsync(NCreature actorNode, ActionModel requestedAction)
    {
        const int maxAttempts = 30;
        const int delayMilliseconds = 50;
        string? firstBlocker = null;
        string? lastBlocker = null;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var blocker = GetReadyBlocker(actorNode, requestedAction);
            if (blocker == null)
            {
                if (firstBlocker != null)
                    Entry.Logger.Info($"{Module} became ready after wait actor={DescribeCreature(actorNode)} firstBlocker={firstBlocker}");

                return true;
            }

            firstBlocker ??= blocker;
            lastBlocker = blocker;

            if (attempt == 0)
                Entry.Logger.Info($"{Module} waiting actor={DescribeCreature(actorNode)} action={requestedAction.Id.Entry} blocker={blocker}");

            await Task.Delay(delayMilliseconds);
        }

        Entry.Logger.Info(
            $"{Module} rejected after wait actor={DescribeCreature(actorNode)} action={requestedAction.Id.Entry} firstBlocker={firstBlocker} lastBlocker={lastBlocker}");
        return false;
    }

    private static string? GetReadyBlocker(NCreature actorNode, ActionModel requestedAction)
    {
        if (!GodotObject.IsInstanceValid(actorNode))
            return "actorNode invalid";

        var actor = actorNode.Entity;
        if (!actor.IsAlive || actor.CombatId == null)
            return $"actor invalid alive={actor.IsAlive} combatId={actor.CombatId?.ToString() ?? "null"}";

        if (!CombatManager.Instance.IsInProgress)
            return "combat not in progress";

        if (CombatManager.Instance.PlayerActionsDisabled)
            return "player actions disabled";

        var queueState = RunManager.Instance.ActionQueueSynchronizer.CombatState;
        if (queueState != ActionSynchronizerCombatState.PlayPhase)
            return $"queue state {queueState}";

        if (actor.PetOwner != null && !LocalContext.IsMe(actor.PetOwner))
            return "non-local pet owner";

        if (actor.IsPlayer && !LocalContext.IsMe(actor))
            return "non-local player actor";

        if (actor.CombatState == null)
            return "null actor combat state";

        if (actor.CombatState.CurrentSide != actor.Side)
            return $"wrong side current={actor.CombatState.CurrentSide} actor={actor.Side}";

        var actionPower = FindMatchingAction(actor, requestedAction);
        if (actionPower == null)
            return $"missing action {requestedAction.Id.Entry}";

        if (!actionPower.CanAct(actor.CombatState))
            return $"action cannot act amount={actionPower.Amount} ownerMatches={actionPower.Owner == actor}";

        return null;
    }

    private static async Task TargetAndEnqueueAsync(NCreature actorNode, ActionModel actionPower, bool useController)
    {
        var actor = actorNode.Entity;
        if (actor.CombatId == null)
            return;

        var targetManager = NTargetManager.Instance;
        var targetType = actionPower.TargetType;
        var targetMode = useController ? TargetMode.Controller : TargetMode.ClickMouseToTarget;
        var startPosition = actorNode.Hitbox.GlobalPosition + actorNode.Hitbox.Size / 2f;

        Entry.Logger.Info($"{Module} start targeting actor={DescribeCreature(actorNode)} action={DescribeAction(actionPower)} targetType={targetType}");
        actionPower.StartPulsing();

        try
        {
            if (CustomTargetTypeManager.IsCustomTargetType(targetType) &&
                CustomTargetTypeManager.TryGetCustomTargetType(targetType, out var customTargetType))
            {
                targetManager.StartTargeting(MinionTargetTypes.AnyCreature, startPosition, targetMode,
                    () => !GodotObject.IsInstanceValid(actorNode) || !actor.IsAlive,
                    node =>
                    {
                        if (node is not NCreature creatureNode)
                            return false;

                        return customTargetType.IsValidTarget(actionPower, creatureNode.Entity);
                    });
            }
            else
            {
                targetManager.StartTargeting(targetType, startPosition, targetMode,
                    () => !GodotObject.IsInstanceValid(actorNode) || !actor.IsAlive, null);
            }

            var selectedNode = await targetManager.SelectionFinished();
            if (selectedNode is not NCreature targetNode)
            {
                Entry.Logger.Info($"{Module} targeting canceled actor={DescribeCreature(actorNode)} action={DescribeAction(actionPower)}");
                return;
            }

            var target = targetNode.Entity;
            if (actor.CombatState == null || !actionPower.IsValidTarget(actor.CombatState, target))
            {
                Entry.Logger.Info($"{Module} targeting rejected invalid target actor={DescribeCreature(actorNode)} action={DescribeAction(actionPower)} target={target.Name}");
                return;
            }

            Entry.Logger.Info($"{Module} targeting selected actor={DescribeCreature(actorNode)} action={DescribeAction(actionPower)} target={target.Name}");
            TryEnqueue(actionPower, target);
        }
        finally
        {
            actionPower.StopPulsing();
        }
    }

    private static bool TryEnqueue(ActionModel actionPower, Creature? target)
    {
        actionPower.Flash();

        if (MinionLibTryEnqueueMethod != null)
        {
            try
            {
                var result = (bool)(MinionLibTryEnqueueMethod.Invoke(null, [actionPower, target]) ?? false);
                Entry.Logger.Info(
                    $"{Module} enqueue via MinionLib result={result} action={DescribeAction(actionPower)} target={target?.Name ?? "null"} queue={RunManager.Instance.ActionQueueSynchronizer.CombatState}");
                if (result)
                    return true;

                Entry.Logger.Info(
                    $"{Module} MinionLib enqueue returned false; bypassing threshold with direct synchronized enqueue action={DescribeAction(actionPower)}");
            }
            catch (Exception ex)
            {
                Entry.Logger.Error($"{Module} MinionLib enqueue failed action={DescribeAction(actionPower)} error={ex}");
            }
        }

        try
        {
            var queuedAction = new ExecuteCreatureActionGameAction(actionPower, target);
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(queuedAction);
            Entry.Logger.Info(
                $"{Module} enqueue direct result=True action={DescribeAction(actionPower)} target={target?.Name ?? "null"} queue={RunManager.Instance.ActionQueueSynchronizer.CombatState}");
            return true;
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"{Module} direct enqueue failed action={DescribeAction(actionPower)} error={ex}");
            return false;
        }
    }

    private static int ConnectInputControls(NCreature actorNode)
    {
        var connected = 0;
        var seen = new HashSet<ulong>();

        connected += ConnectInputControl(actorNode.Hitbox, actorNode, seen) ? 1 : 0;
        connected += ConnectInputControlsUnder(actorNode, actorNode, seen);

        return connected;
    }

    private static int ConnectInputControlsUnder(Node root, NCreature actorNode, HashSet<ulong> seen)
    {
        var connected = 0;

        foreach (var child in root.GetChildren())
        {
            if (child is NPower)
                continue;

            if (child is Control control)
                connected += ConnectInputControl(control, actorNode, seen) ? 1 : 0;

            connected += ConnectInputControlsUnder(child, actorNode, seen);
        }

        return connected;
    }

    private static bool ConnectInputControl(Control control, NCreature actorNode, HashSet<ulong> seen)
    {
        if (control is NPower || !seen.Add(control.GetInstanceId()))
            return false;

        control.Connect(Control.SignalName.GuiInput,
            Callable.From<InputEvent>(inputEvent => OnHitboxGuiInput(actorNode, inputEvent)));
        return true;
    }

    private static bool IsSupportedMinion(NCreature creatureNode)
    {
        return creatureNode.Entity.Monster is FloatingSeaMinion or HarvestMinion;
    }

    internal static bool IsSupportedAction(ActionModel action)
    {
        return action is FloatingSeaBlockAction or HarvestAttackAction;
    }

    private static ActionModel? FindMatchingAction(Creature actor, ActionModel requestedAction)
    {
        return actor.Powers
            .OfType<ActionModel>()
            .FirstOrDefault(power => power.Id == requestedAction.Id && IsSupportedAction(power));
    }

    private static bool IsSingleTarget(TargetType targetType)
    {
        return CustomTargetTypeManager.TryGetCustomTargetType(targetType, out var customTargetType)
            ? customTargetType.IsSingleTarget
            : targetType.IsSingleTarget();
    }

    private static string DescribeCreature(NCreature creatureNode)
    {
        var creature = creatureNode.Entity;
        return $"{creature.Name}/{creature.CombatId?.ToString() ?? "no-combat-id"}/{creature.Monster?.GetType().Name ?? "no-monster"}";
    }

    internal static string DescribeAction(ActionModel? action)
    {
        return action == null ? "none" : $"{action.Id.Entry}:{action.Amount}:{action.GetType().Name}";
    }

    private static string DescribeActions(Creature creature)
    {
        return string.Join(", ", creature.Powers.OfType<ActionModel>().Select(DescribeAction));
    }
}

[HarmonyPatch(typeof(NPower), nameof(NPower._Ready))]
public static class MinionActionIconFallbackPatch
{
    private const string Module = "[MinionActionIconFallback]";
    private const int MaxModelReadyAttempts = 4;

    [HarmonyPostfix]
    private static void Postfix(NPower __instance)
    {
        TryConnectWhenModelReady(__instance, 0);
    }

    private static void TryConnectWhenModelReady(NPower powerNode, int attempt)
    {
        if (!GodotObject.IsInstanceValid(powerNode))
            return;

        ActionModel? actionPower;
        try
        {
            actionPower = powerNode.Model as ActionModel;
        }
        catch (InvalidOperationException)
        {
            if (attempt < MaxModelReadyAttempts)
            {
                Callable.From(() => TryConnectWhenModelReady(powerNode, attempt + 1)).CallDeferred();
            }
            return;
        }

        if (actionPower == null ||
            !MinionActionClickFallbackPatch.IsSupportedAction(actionPower))
            return;

        powerNode.Connect(Control.SignalName.GuiInput,
            Callable.From<InputEvent>(inputEvent => OnPowerGuiInput(powerNode, inputEvent)));
        Entry.Logger.Info($"{Module} connected action={MinionActionClickFallbackPatch.DescribeAction(actionPower)}");
    }

    private static void OnPowerGuiInput(NPower powerNode, InputEvent inputEvent)
    {
        if (NTargetManager.Instance.IsInSelection)
            return;

        var triggeredByMouse =
            inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouseButton &&
            mouseButton.IsReleased();

        if (!triggeredByMouse || powerNode.Model is not ActionModel actionPower)
            return;

        if (!MinionActionClickFallbackPatch.IsSupportedAction(actionPower))
            return;

        var actorNode = NCombatRoom.Instance?.GetCreatureNode(actionPower.Owner);
        if (actorNode == null)
        {
            Entry.Logger.Info($"{Module} ignored missing actor node action={MinionActionClickFallbackPatch.DescribeAction(actionPower)}");
            return;
        }

        Entry.Logger.Info($"{Module} input action={MinionActionClickFallbackPatch.DescribeAction(actionPower)}");
        TaskHelper.RunSafely(MinionActionClickFallbackPatch.TryUseActionAsync(actorNode, actionPower, false));
        powerNode.GetViewport().SetInputAsHandled();
    }
}
