using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Enemies;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class IzumikOffspringGuardPower : ModPowerTemplate
{
    private bool appliedOpeningGuard;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/IzumikOffspringGuardPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/IzumikOffspringGuardPower.png";

    public override async Task BeforeCombatStart()
    {
        if (appliedOpeningGuard || Owner.Monster is not Izumik izumik)
            return;

        appliedOpeningGuard = true;
        await izumik.ApplyOpeningOffspringGuard();
    }

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal unblockedDamage, ValueProp props, Creature? dealer)
    {
        if (target != Owner || unblockedDamage <= 0 || Owner.CombatState == null)
            return target;

        List<Creature> offspring = Owner.CombatState.Enemies
            .Where(enemy => enemy.IsAlive && enemy.Monster is IzumikOffspring)
            .ToList();

        if (offspring.Count == 0)
            return target;

        Flash();
        return Owner.CombatState.RunState.Rng.CombatTargets.NextItem(offspring)!;
    }
}
