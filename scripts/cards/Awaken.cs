using STS2RitsuLib.Cards.DynamicVars;
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
using Arknights_Mizuki.Scripts.Powers;

namespace Arknights_Mizuki.Scripts.Cards;

// 注册卡牌。如果要写自定义池看添加人物的开头
[RegisterCard(typeof(MzkCardPool))]
[RegisterArchaicToothTranscendence(typeof(AwakenPlus))]
public class Awaken : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Basic;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AllEnemies;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		(DynamicVar)new PowerVar<SanityPower>(2m)
	};

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[2]
    {
        HoverTipFactory.FromPower<SanityPower>(),
        HoverTipFactory.FromPower<SanityBurstDescriptionPower>()
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/Awaken.png";

    public Awaken() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue) // 造成伤害，数值来源于卡牌的基础伤害属性
            .FromCard(this,cardPlay) // 伤害来源于这张卡牌
            .TargetingAllOpponents(((CardModel)this).CombatState)
            .Execute(choiceContext);
        var opponents = ((CardModel)this).CombatState
            .GetOpponentsOf(Owner.Creature)
            .Where(opponent => opponent.IsAlive)
            .ToList();
        foreach (var opponent in opponents)
        {
            if(!opponent.IsAlive)continue;
            await PowerCmd.Apply<SanityPower>(
                choiceContext, 
                opponent, 
                ((DynamicVar)((CardModel)this).DynamicVars["SanityPower"]).BaseValue, 
                ((CardModel)this).Owner.Creature, 
                (CardModel)(object)this, 
                false
            );
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
		((DynamicVar)((CardModel)this).DynamicVars["SanityPower"]).UpgradeValueBy(1m);
    }
}
