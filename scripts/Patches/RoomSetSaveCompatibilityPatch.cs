using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(RoomSet), nameof(RoomSet.FromSave))]
public static class RoomSetSaveCompatibilityPatch
{
    private static void Prefix(SerializableRoomSet save)
    {
        save.EventIds ??= new List<ModelId>();
        save.NormalEncounterIds ??= new List<ModelId>();
        save.EliteEncounterIds ??= new List<ModelId>();
    }
}
