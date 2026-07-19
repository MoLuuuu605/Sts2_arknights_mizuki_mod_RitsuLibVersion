using HarmonyLib;
using Arknights_Mizuki.Scripts.Acts;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(ActModel), nameof(ActModel.CreateMap))]
public static class ActMapCreationPatch
{
    public static bool Prefix(ActModel __instance, RunState runState, bool replaceTreasureWithElites, ref ActMap __result)
    {
        if (__instance is EvolutionSingularityAct)
        {
            __result = new EvolutionSingularityMap();
            return false;
        }
        return true;
    }
}
