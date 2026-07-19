using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Rooms;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(MultiplayerScalingModel), nameof(MultiplayerScalingModel.GetMultiplayerScaling))]
public static class MultiplayerScalingPatch
{
    private static bool Prefix(EncounterModel? encounter, int actIndex, ref decimal __result)
    {
        if (actIndex <= 2)
        {
            return true;
        }

        __result = encounter?.RoomType == RoomType.Boss ? 1.3m : 1.2m;
        return false;
    }
}
