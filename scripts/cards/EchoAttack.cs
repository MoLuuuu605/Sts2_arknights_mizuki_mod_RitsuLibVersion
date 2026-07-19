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
using Arknights_Mizuki.Scripts.StatusSlots;
using Arknights_Mizuki.Scripts.keywords;

namespace Arknights_Mizuki.Scripts.Cards;

// 注册卡牌。如果要写自定义池看添加人物的开头
[RegisterCard(typeof(MzkCardPool))]
public class EchoAttack : ModCardTemplate
{
    private const string EchoCountdownKey = "EchoCountdown";
    // 基础耗能
    private const int energyCost = 1;
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		(DynamicVar)new PowerVar<SanityPower>(1m),
        (DynamicVar)new DynamicVar(EchoCountdownKey, 1m)
	};

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
	{
        HoverTipFactory.FromKeyword(Echo1.Echo)
	};

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/EchoAttack.png";
    public EchoAttack() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return;

        if (!StatusSlotManager.IsPlayerEligibleForSlot(Owner, StatusSlotType.SwarmCall))
            return;

        if (this.Pile.Type == PileType.Hand){
        DynamicVars.Damage.BaseValue += 1;
        DynamicVars[EchoCountdownKey].BaseValue = 1;
        }
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(2) // 造成伤害，数值来源于卡牌的基础伤害属性
            .FromCard(this,cardPlay) // 伤害来源于这张卡牌
            .Targeting(cardPlay.Target) // 伤害目标是玩家选择的目标
            .Execute(choiceContext);
        if(cardPlay.Target.IsAlive)
        await PowerCmd.Apply<SanityPower>(choiceContext, cardPlay.Target, ((DynamicVar)((CardModel)this).DynamicVars["SanityPower"]).BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
    }
}
