using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class GoldLifeReturnPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;
    public override bool ShouldReceiveCombatHooks => true;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/GoldLifeReturnPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/GoldLifeReturnPower.png";
    
    int MaxHp= 80;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        MaxHp=Owner.MaxHp;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        var CurrentMaxHp=Owner.MaxHp;
        if(CurrentMaxHp < MaxHp)
        Owner.SetMaxHpInternal(MaxHp);
        else MaxHp=CurrentMaxHp;
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var CurrentMaxHp=Owner.MaxHp;
        if(CurrentMaxHp < MaxHp)
        Owner.SetMaxHpInternal(MaxHp);
        else MaxHp=CurrentMaxHp;
    }
}
