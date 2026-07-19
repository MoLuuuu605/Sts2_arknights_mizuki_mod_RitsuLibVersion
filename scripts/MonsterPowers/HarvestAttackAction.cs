using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.Utils;
using Arknights_Mizuki.RitsuAdapters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Arknights_Mizuki.Scripts.Actions;

[RegisterPower]
public sealed class HarvestAttackAction : ModActionTemplate
{
    public override TargetType TargetType => TargetType.AllEnemies;
    private const int BaseDamage = 4;
    private const int BaseRepeats = 2;
    public override bool DecrementAfterAct => true;
    public override bool OnlyRespondIconClick => true;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/HarvestAttackAction.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/HarvestAttackAction.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        (DynamicVar)new DamageVar(BaseDamage, ValueProp.Unpowered),
        new DynamicVar("Repeat", BaseRepeats)
    ];

    public override LocString Description => AddDynamicDescriptionVars(base.Description);

    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        var actor = Owner;
        var damage = GetDamageAmount();
        var repeats = GetRepeatCount();
        Entry.Logger.Info($"[MinionAction] HarvestAttackAction act actor={actor.Name} damage={damage} repeats={repeats}");
        if (MinionAnimationHelper.MarkHarvestSkillBegun(actor))
        {
            MinionAnimationHelper.Play(actor, "Buff");
            await Task.Delay(500);
        }
        MinionAnimationHelper.Play(actor, "Attack");
        var enemies = CombatState.GetOpponentsOf(Owner).ToList(); // 需要确认实际API
        for(int i=0;i<repeats;i++)
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.IsDead)
                {
                    await CreatureCmd.Damage(choiceContext, enemy, damage, ValueProp.Unpowered, null, null);
                }
            }
        }
        await CreatureCmd.Damage(choiceContext,Owner,1,ValueProp.Unblockable|ValueProp.Unpowered,null!,null);
    }

    private LocString AddDynamicDescriptionVars(LocString locString)
    {
        locString.Add("Damage", GetDamageAmount());
        locString.Add("Repeat", GetRepeatCount());
        return locString;
    }

    private decimal GetDamageAmount() => BaseDamage + GetSeabornizationAmount();

    private static int GetRepeatCount() => BaseRepeats;

    private decimal GetSeabornizationAmount()
    {
        var owner = IsMutable ? Owner : null;
        return owner?.Powers.OfType<SeabornizationPower>().Sum(power => power.Amount) ?? 0m;
    }
}
