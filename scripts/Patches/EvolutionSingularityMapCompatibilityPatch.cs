using Arknights_Mizuki.Scripts.Acts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(RunManager), "InitializeSavedRun")]
public static class EvolutionSingularityRunManagerSavedMapPatch
{
    private static void Prefix(SerializableRun save)
    {
        EvolutionSingularityMapSaveCompatibility.Fix(save);
    }
}

[HarmonyPatch(typeof(RunState), nameof(RunState.FromSerializable))]
public static class EvolutionSingularityRunStateSavedMapPatch
{
    private static void Prefix(SerializableRun save)
    {
        EvolutionSingularityMapSaveCompatibility.Fix(save);
    }
}

internal static class EvolutionSingularityMapSaveCompatibility
{
    public static void Fix(SerializableRun? save)
    {
        if (save?.Acts == null)
        {
            return;
        }

        ModelId actId = ModelDb.Act<EvolutionSingularityAct>().Id;
        foreach (SerializableActModel act in save.Acts)
        {
            if (act.Id != actId)
            {
                continue;
            }

            FixSavedMap(act.SavedMap);
        }
    }

    private static void FixSavedMap(SerializableActMap? map)
    {
        if (map?.BossPoint == null)
        {
            return;
        }

        MapCoord bossCoord = map.BossPoint.Coord;
        if (map.GridHeight <= 0 || bossCoord.row != map.GridHeight - 1)
        {
            return;
        }

        bool bossIsOnlyReferencedAsTerminal = map.Points?.Any(point =>
            point.ChildCoords?.Any(child => child.col == bossCoord.col && child.row == bossCoord.row) == true) == true;
        if (!bossIsOnlyReferencedAsTerminal)
        {
            return;
        }

        map.GridHeight = bossCoord.row;
        Entry.Logger.Info($"[EvolutionSingularityMap] Fixed saved boss row layout: boss=({bossCoord.col},{bossCoord.row}) height={map.GridHeight}");
    }
}
