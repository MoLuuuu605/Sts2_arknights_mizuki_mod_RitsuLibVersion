using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class AberrantRegenerationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/AberrantRegenerationPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/AberrantRegenerationPower.png";


    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if(target != Owner || result.UnblockedDamage <=0)
            return;
        for(int i=0;i<Amount;i++)
        {
            await CreatureCmd.GainBlock(Owner,result.UnblockedDamage,ValueProp.Move,null);
        }

    }
}
