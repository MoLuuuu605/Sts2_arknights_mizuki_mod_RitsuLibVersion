using Arknights_Mizuki.Scripts.Events;
using Arknights_Mizuki.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyUnknownMapPointRoomTypes))]
public static class SublimationUnknownRoomPatch
{
    private static void Postfix(IRunState runState, ref IReadOnlySet<RoomType> __result)
    {
        if (SublimationEventPatchUtil.ShouldForceSublimation(runState))
        {
            __result = new HashSet<RoomType> { RoomType.Event };
        }
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyNextEvent))]
public static class SublimationNextEventPatch
{
    private static void Postfix(IRunState runState, ref EventModel __result)
    {
        if (SublimationEventPatchUtil.ShouldForceSublimation(runState) || SublimationEventPatchUtil.IsSublimationPending(runState))
        {
            __result = ModelDb.Event<Sublimation>();
        }
    }
}

[HarmonyPatch(typeof(RunManager), "RollRoomTypeFor")]
public static class SublimationRollRoomTypePatch
{
    private static bool Prefix(RunManager __instance, MapPointType pointType, ref RoomType __result)
    {
        RunState? state = __instance.DebugOnlyGetState();
        if (pointType == MapPointType.Unknown && state != null && SublimationEventPatchUtil.IsSublimationPending(state))
        {
            __result = RoomType.Event;
            return false;
        }

        return true;
    }
}

internal static class SublimationEventPatchUtil
{
    public static bool ShouldForceSublimation(IRunState runState)
    {
        return IsSublimationPending(runState)
            && runState is RunState state
            && state.CurrentMapPoint?.PointType == MapPointType.Unknown;
    }

    public static bool IsSublimationPending(IRunState runState)
    {
        if (runState is not RunState state)
        {
            return false;
        }

        if (state.CurrentActIndex != 2)
        {
            return false;
        }

        if (state.VisitedEventIds.Contains(ModelDb.Event<Sublimation>().Id))
        {
            return false;
        }

        if (ActUnlockHelper.HasDetermination(state))
        {
            return false;
        }

        return true;
    }
}
