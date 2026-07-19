using Arknights_Mizuki.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
public static class FourthActPatch
{
    private static void Prefix(RunManager __instance)
    {
        RunState? state = __instance.DebugOnlyGetState();
        if (state == null)
        {
            return;
        }

        if (state.CurrentActIndex == state.Acts.Count - 1 && state.CurrentActIndex == 2 && ActUnlockHelper.HasDetermination(state))
        {
            ActUnlockHelper.EnsureFourthActUnlocked(state);
        }
    }
}
