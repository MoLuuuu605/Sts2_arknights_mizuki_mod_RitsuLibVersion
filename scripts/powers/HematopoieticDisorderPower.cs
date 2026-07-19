using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
public sealed class HematopoieticDisorderPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath =>
        "res://Arknights_Mizuki/images/powers/hematopoietic_disorder.png";

    public override string CustomBigIconPath => CustomIconPath;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side != CombatSide.Player || !participants.Contains(Owner) || Owner.MaxHp <= 0 ||
            (decimal)Owner.CurrentHp >= Owner.MaxHp * 0.8m)
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            1,
            ValueProp.Unblockable,
            null,
            null);
    }
}
