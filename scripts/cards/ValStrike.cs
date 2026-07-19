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
using MegaCrit.Sts2.Core.Models.Powers;

namespace Arknights_Mizuki.Scripts.Cards;

// 注册卡牌。如果要写自定义池看添加人物的开头
[RegisterCard(typeof(MzkCardPool))]
public class ValStrike : ModCardTemplate
{
    // 基础耗能\

    private const int energyCost = 1;
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(7m, (ValueProp)8),
		(DynamicVar)new PowerVar<VulnerablePower>(1m)
	};

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
	{
		HoverTipFactory.FromPower<VulnerablePower>()
	};

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/VulStrike.png";

    public ValStrike() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue) // 造成伤害，数值来源于卡牌的基础伤害属性
            .FromCard(this,cardPlay) // 伤害来源于这张卡牌
            .Targeting(cardPlay.Target) // 伤害目标是玩家选择的目标
            .Execute(choiceContext);
        if(cardPlay.Target.IsAlive)
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, ((DynamicVar)((CardModel)this).DynamicVars["VulnerablePower"]).BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
		((DynamicVar)((CardModel)this).DynamicVars["VulnerablePower"]).UpgradeValueBy(1m);
    }
}
