using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
/// 损伤爆发，当爆发损伤时获得一层反移情;
/// </summary>
public sealed class SanityBurstDescriptionPower: ModPowerTemplate
{   
    private const int BaseDamageAtActOne = 20;
    private const int DamagePerAdditionalAct = 10;
    private const int DamageGrowth = 15;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/Sanity.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/Sanity.png";

    public override LocString Description => AddDynamicDescriptionVars(base.Description);

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DamageGrowth", DamageGrowth),
        new DynamicVar("BaseDamage", BaseDamageAtActOne)
        ];

    public override Task BeforeCombatStart()
    {
        DynamicVars["DamageGrowth"].BaseValue = DamageGrowth;
        var damage = GetBaseDamage(Owner);
        DynamicVars["BaseDamage"].BaseValue = damage;
        return Task.CompletedTask;
    }

    private LocString AddDynamicDescriptionVars(LocString locString)
    {
        locString.Add("DamageGrowth", DamageGrowth);
        locString.Add("BaseDamage", GetBaseDamage(IsMutable ? Owner : null));
        return locString;
    }

    private static int GetBaseDamage(Creature? owner)
    {
        int currentActIndex = owner?.CombatState?.Players.FirstOrDefault()?.RunState.CurrentActIndex
            ?? RunManager.Instance.DebugOnlyGetState()?.CurrentActIndex
            ?? 0;

        return BaseDamageAtActOne + Math.Max(0, currentActIndex) * DamagePerAdditionalAct;
    }
}
