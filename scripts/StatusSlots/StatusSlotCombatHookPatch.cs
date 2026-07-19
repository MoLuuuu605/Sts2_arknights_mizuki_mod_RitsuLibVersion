using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.StatusSlots;

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class StatusSlotEnsureModifierBeforeCombatPatch
{
    private static void Prefix(IRunState runState, ICombatState combatState)
    {
        if (runState is not RunState concreteRunState)
            return;

        StatusSlotManager.EnsureData(concreteRunState);
        StatusSlotManager.AttachModifier(concreteRunState);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered), typeof(IRunState), typeof(AbstractRoom))]
public static class StatusSlotRoomSettlementFallbackPatch
{
    private static void Postfix(IRunState __0, AbstractRoom __1, ref Task __result)
    {
        __result = ContinueAfterRoomEnteredAsync(__result, __0, __1);
    }

    private static async Task ContinueAfterRoomEnteredAsync(
        Task originalTask,
        IRunState runState,
        AbstractRoom room)
    {
        await originalTask;
        if (runState is not RunState concreteRunState)
            return;

        StatusSlotManager.EnsureData(concreteRunState);
        await StatusSlotManager.SettleRoomEnteredAsync(concreteRunState, room);
        await StatusSlotManager.SettleEchoModificationRoomEnteredAsync(concreteRunState, room);
    }
}
