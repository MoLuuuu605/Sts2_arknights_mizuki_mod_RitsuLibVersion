using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public class SanityBurstStrengthDebuffPower : ModPowerTemplate
{
    public override PowerType Type =>PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/Sanity.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/Sanity.png";

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var choiceContext = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(choiceContext,Owner,-Amount,applier,null,true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if(side != Owner.Side)
        return;
        await PowerCmd.Apply<StrengthPower>(choiceContext,Owner,Amount,Owner,null,true);
        await PowerCmd.Remove<SanityBurstStrengthDebuffPower>(Owner);
    }
}
