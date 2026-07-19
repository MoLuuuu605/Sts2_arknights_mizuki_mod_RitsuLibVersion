using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

using Arknights_Mizuki.Scripts.Pools;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class CutDown : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new DamageVar(3m, ValueProp.Move)
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/Cutdown.png";

    public CutDown() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 造成伤害并捕获实际伤害量
        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        // 获取实际造成的伤害（考虑易伤等修正）
        decimal actualDamage = attackCommand.Results.SelectMany(r => r).Sum(r => r.TotalDamage);
        if (actualDamage <= 0) return;

        // 获取活着的召唤物
        var alivePets = Owner.PlayerCombatState?.Pets
            .Where(pet => pet is { IsAlive: true, IsPet: true })
            .ToList();

        if (alivePets == null || alivePets.Count == 0) return;

        if (IsUpgraded)
        {
            // 升级后：治疗所有召唤物
            foreach (var pet in alivePets)
            {
                await CreatureCmd.Heal(pet, actualDamage);
            }
        }
        else
        {
            // 基础：随机治疗1个召唤物
            var randomPet = CombatState.RunState.Rng.CombatTargets.NextItem(alivePets);
                if (randomPet != null)
            {
                await CreatureCmd.Heal(randomPet, actualDamage);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}