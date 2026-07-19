using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class PainAmplifyPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string? CustomIconPath => "res://Arknights_Mizuki/images/powers/Sanity_Buff.png";
    public override string? CustomBigIconPath => "res://Arknights_Mizuki/images/powers/Sanity_buff.png";

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);

        if (Amount <= 0) return;
        if (!props.IsPoweredAttack()) return;
        if (target != Owner) return;

        await PowerCmd.Apply<SanityPower>(
            choiceContext,
            target,
            Amount,
            dealer ?? target,
            null,
            false
        );
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await PowerCmd.Remove<PainAmplifyPower>(Owner);
    }
}
