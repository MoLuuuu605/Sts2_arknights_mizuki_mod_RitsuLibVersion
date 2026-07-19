using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class HiveWillPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/HiveWillPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/HiveWillPower.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player.Creature || Amount <= 0)
            return;

        await ApplyToMinions(choiceContext, player, Amount);
        Flash();
    }

    public static async Task ApplyToMinions(PlayerChoiceContext choiceContext, Player player, decimal amount)
    {
        var minions = player.PlayerCombatState?.Pets
            .Where(pet => pet is { IsAlive: true, IsPet: true })
            .ToList();

        if (minions == null)
            return;

        foreach (var minion in minions)
        {
            await PowerCmd.Apply<SeabornizationPower>(choiceContext, minion, amount, minion, null);
        }
    }
}
