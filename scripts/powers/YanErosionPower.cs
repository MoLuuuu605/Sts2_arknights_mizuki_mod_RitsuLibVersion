using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
public sealed class YanErosionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath =>
        "res://Arknights_Mizuki/images/powers/yan_erosion.png";

    public override string CustomBigIconPath => CustomIconPath;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);
        if (side != CombatSide.Player || Owner.Player == null)
            return;

        Flash();
        await PlayerCmd.GainEnergy(1m, Owner.Player);
    }
}
