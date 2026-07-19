using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
/// </summary>
public sealed class SanityBuffPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/Sanity_Buff.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/Sanity_Buff.png";
    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        await base.AfterAttack(choiceContext, command);
        
        if (Amount <= 0) return;
        if (command.Attacker != Owner) return;
        
        foreach (var hitResults in command.Results)
        {
            foreach (var damageResult in hitResults)
            {
                var target = damageResult.Receiver;
                if(target == Owner)
                {
                    return;
                }
                if (target != null && target.IsAlive && target != Owner)
                {
                    await PowerCmd.Apply<SanityPower>(choiceContext, target, Amount, Owner, null, false);
                }
            }
        }
        if(Amount>=0)
        {
        await PowerCmd.Remove<SanityBuffPower>(Owner);
        }

        Flash();
    }
}