using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class ColdWavePower : ModPowerTemplate
{
    public const int HitsPerCold = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ColdPower>()
    };

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/ColdWavePower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/ColdWavePower.png";

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || result.WasFullyBlocked || cardSource?.Type != CardType.Attack)
            return;

        int remaining = await PowerCmd.ModifyAmount(choiceContext, this, -1, Owner, cardSource);
        if (remaining > 0)
            return;

        foreach (var player in Owner.CombatState.Players.Where(player => player.Creature.IsAlive))
        {
            await PowerCmd.Apply<ColdPower>(choiceContext, player.Creature, 1, Owner, cardSource, false);
        }

        Flash();
        await PowerCmd.Apply<ColdWavePower>(choiceContext, Owner, HitsPerCold, Owner, null, false);
    }
}
