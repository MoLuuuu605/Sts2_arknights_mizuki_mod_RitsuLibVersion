using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
public sealed class EchoStrifePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath =>
        "res://Arknights_Mizuki/images/powers/echo_strife.png";

    public override string CustomBigIconPath => CustomIconPath;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);
        if (side != CombatSide.Player)
            return;

        Flash();
        var choiceContext = new ThrowingPlayerChoiceContext();
        foreach (Creature ally in combatState.Allies)
            if (ally.IsAlive)
                await PowerCmd.Apply<StrengthPower>(choiceContext, ally, 1, Owner, null, false);
        foreach (Creature enemy in combatState.Enemies)
            if (enemy.IsAlive)
                await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, 1, Owner, null, false);
    }
}
