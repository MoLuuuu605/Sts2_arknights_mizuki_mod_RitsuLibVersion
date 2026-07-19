using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
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
