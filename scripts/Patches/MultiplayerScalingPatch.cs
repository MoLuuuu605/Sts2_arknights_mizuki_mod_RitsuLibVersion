using HarmonyLib;
using Arknights_Mizuki.Scripts.Acts;
using Arknights_Mizuki.Scripts.Settings;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(MultiplayerScalingModel), nameof(MultiplayerScalingModel.GetMultiplayerScaling))]
public static class MultiplayerScalingPatch
{
    private static bool Prefix(EncounterModel? encounter, int actIndex, ref decimal __result)
    {
        RunState? runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null ||
            actIndex < 0 ||
            actIndex >= runState.Acts.Count ||
            runState.Acts[actIndex] is not EvolutionSingularityAct)
        {
            return true;
        }

        __result = FourthActSettings.GetMultiplayerScaling(
            runState,
            encounter?.RoomType ?? RoomType.Monster);
        return false;
    }
}
