using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.StatusSlots;
using Arknights_Mizuki.Scripts.keywords;

namespace Arknights_Mizuki.Scripts.Cards;

// 注册卡牌。如果要写自定义池看添加人物的开头
[RegisterCard(typeof(MzkCardPool))]
public class EchoSanity : ModCardTemplate
{
    private const string EchoCountdownKey = "EchoCountdown";
    private const int EchoThreshold = 2;
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		(DynamicVar)new PowerVar<SanityPower>(4m),
        (DynamicVar)new DynamicVar(EchoCountdownKey, EchoThreshold)
	};

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[3]
	{
		HoverTipFactory.FromPower<SanityPower>(),
HoverTipFactory.FromPower<SanityBurstDescriptionPower>(),
        HoverTipFactory.FromKeyword(Echo2.Echo)
	};

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/EchoSanity.png";
    public EchoSanity() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return;

        if (!StatusSlotManager.IsPlayerEligibleForSlot(Owner, StatusSlotType.SwarmCall))
            return;

        if (this.Pile.Type == PileType.Hand){
            DynamicVars[EchoCountdownKey].BaseValue -= 1;
            if(DynamicVars[EchoCountdownKey].IntValue <= 0)
            {
                DynamicVars["SanityPower"].BaseValue += 1;
                DynamicVars[EchoCountdownKey].BaseValue = EchoThreshold;
            }
        }
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SanityPower>(choiceContext, cardPlay.Target, ((DynamicVar)((CardModel)this).DynamicVars["SanityPower"]).BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
		((CardModel)this).DynamicVars["SanityPower"].UpgradeValueBy(2);
    }
}
