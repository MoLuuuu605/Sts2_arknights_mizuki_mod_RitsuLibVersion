using System.Reflection;
using Arknights_Mizuki.Scripts.Acts;
using Arknights_Mizuki.Scripts.Ancients;
using Arknights_Mizuki.Scripts.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Utils;

public static class ActUnlockHelper
{
    private static readonly FieldInfo? ActsBackingField = typeof(RunState).GetField(
        "<Acts>k__BackingField",
        BindingFlags.Instance | BindingFlags.NonPublic);

    public static bool HasDetermination(RunState runState)
    {
        return runState.Players.Any(HasDetermination);
    }

    public static bool HasDetermination(Player player)
    {
        return player.Relics.Any(relic => relic is Determination);
    }

    public static bool EnsureFourthActUnlocked(RunState runState)
    {
        ActModel? existingFourthAct = runState.Acts.FirstOrDefault(act => act is EvolutionSingularityAct);
        if (existingFourthAct != null)
        {
            EnsureRoomsGenerated(existingFourthAct, runState);
            return false;
        }

        List<ActModel> acts = runState.Acts.ToList();
        ActModel fourthAct = ModelDb.Act<EvolutionSingularityAct>().ToMutable();
        EnsureRoomsGenerated(fourthAct, runState);
        acts.Add(fourthAct);
        ActsBackingField?.SetValue(runState, acts);
        return true;
    }

    private static void EnsureRoomsGenerated(ActModel act, RunState runState)
    {
        if (act is EvolutionSingularityAct && ShouldRegenerateFourthActRooms(act))
        {
            act.GenerateRooms(runState.Rng.UpFront, runState.UnlockState, runState.Players.Count > 1);
            return;
        }

        try
        {
            _ = act.BossEncounter;
        }
        catch (InvalidOperationException)
        {
            act.GenerateRooms(runState.Rng.UpFront, runState.UnlockState, runState.Players.Count > 1);
        }
    }

    private static bool ShouldRegenerateFourthActRooms(ActModel act)
    {
        try
        {
            return act.Ancient is not LastTidewatcher;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }
}
