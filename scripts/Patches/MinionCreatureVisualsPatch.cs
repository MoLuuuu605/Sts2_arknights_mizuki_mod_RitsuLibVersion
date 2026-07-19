using Arknights_Mizuki.Scripts.Minions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Godot;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.CreateVisuals))]
public static class MinionCreatureVisualsPatch
{
    public static bool Prefix(MonsterModel __instance, ref NCreatureVisuals __result)
    {
        string? scenePath = __instance switch
        {
            FloatingSeaMinion => FloatingSeaMinion.VisualsScenePath,
            HarvestMinion => HarvestMinion.VisualsScenePath,
            _ => null
        };

        if (scenePath == null)
            return true;

        __result = RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(scenePath);
        return false;
    }
}
