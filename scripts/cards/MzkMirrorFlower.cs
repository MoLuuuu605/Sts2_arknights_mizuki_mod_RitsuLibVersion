using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

using Arknights_Mizuki.Scripts.Pools;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class MzkMirrorFlower : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new DamageVar(10m, (ValueProp)8),
        (DynamicVar)new HpLossVar(4m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[0];

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/mirror_flower.png";

    public MzkMirrorFlower() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var opponents = ((CardModel)this).CombatState.GetOpponentsOf(Owner.Creature);
        if (opponents.Count() <= 2)
        {
            await CreatureCmd.Damage(
                choiceContext,
                ((CardModel)this).Owner.Creature,
                ((DynamicVar)((CardModel)this).DynamicVars["HpLoss"]).BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered,
                ((CardModel)this).Owner.Creature);
        }
        // 对所有敌人造成6点伤害2次
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(((CardModel)this).CombatState)
            .Execute(choiceContext);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(((CardModel)this).CombatState)
            .Execute(choiceContext);

    }

    protected override void OnUpgrade()
    {
        // 伤害+3，HpLoss-3
        ((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(4m);
        ((DynamicVar)((CardModel)this).DynamicVars["HpLoss"]).UpgradeValueBy(-2m);
    }
}