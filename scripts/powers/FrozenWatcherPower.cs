using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class FrozenWatcherPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/ColdWavePower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/ColdWavePower.png";

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner) || Owner.Player == null)
            return;

        int frozenCount = PileType.Hand.GetPile(Owner.Player).Cards.Count(card => card is Frozen);
        if (frozenCount <= 0)
            return;

        await PowerCmd.Apply<ColdPower>(choiceContext, Owner, frozenCount, Owner, null, false);
        Flash();
    }
}
