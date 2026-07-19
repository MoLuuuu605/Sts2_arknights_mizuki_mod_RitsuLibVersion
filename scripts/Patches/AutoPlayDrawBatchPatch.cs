using Arknights_Mizuki.Scripts.Singletons;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Draw), typeof(PlayerChoiceContext), typeof(decimal), typeof(Player), typeof(bool))]
public static class AutoPlayDrawBatchPatch
{
    private static void Prefix(Player player)
    {
        AutoPlayLimit.BeginDrawBatch(player);
    }

    private static void Postfix(PlayerChoiceContext choiceContext, Player player, ref Task<IEnumerable<CardModel>> __result)
    {
        __result = FinishDrawBatch(__result, choiceContext, player);
    }

    private static async Task<IEnumerable<CardModel>> FinishDrawBatch(
        Task<IEnumerable<CardModel>> drawTask,
        PlayerChoiceContext choiceContext,
        Player player)
    {
        try
        {
            return await drawTask;
        }
        finally
        {
            await AutoPlayLimit.EndDrawBatch(choiceContext, player);
        }
    }
}
