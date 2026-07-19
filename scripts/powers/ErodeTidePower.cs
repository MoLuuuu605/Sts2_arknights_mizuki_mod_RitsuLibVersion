using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class ErodeTidePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/erode_tide.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/erode_tide.png";

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        if (cardPlay.Card.Owner != Owner.Player)
            return;
        
        if (Amount <= 0)
            return;

        var opponents = CombatState.GetOpponentsOf(Owner).ToList();
        foreach (var enemy in opponents)
        {
            if (enemy.IsAlive)
            {
                await PowerCmd.Apply<SanityPower>(
                    choiceContext,
                    enemy,
                    Amount,
                    Owner,
                    cardPlay.Card,
                    false);
            }
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await PowerCmd.Remove<ErodeTidePower>(Owner);
    }
}
