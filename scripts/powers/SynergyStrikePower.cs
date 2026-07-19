using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class SynergyStrikePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string? CustomIconPath => "res://Arknights_Mizuki/images/powers/SynergyStrike.png";
    public override string? CustomBigIconPath => "res://Arknights_Mizuki/images/powers/SynergyStrike.png";

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        await base.AfterAttack(choiceContext, command);

        if (Amount <= 0) return;
        if (command.Attacker != Owner) return;

        foreach (var hitResults in command.Results)
        {
            foreach (var damageResult in hitResults)
            {
                if (damageResult.UnblockedDamage > 0
                    && damageResult.Receiver != null
                    && damageResult.Receiver.IsAlive
                    && damageResult.Receiver != Owner)
                {
                    await PowerCmd.Apply<SanityPower>(
                        choiceContext,
                        damageResult.Receiver,
                        Amount,
                        Owner,
                        null,
                        false
                    );
                }
            }
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await PowerCmd.Remove<SynergyStrikePower>(Owner);
    }
}
