using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;

using Arknights_Mizuki.Scripts.Pools;

namespace Arknights_Mizuki.Scripts.Cards;

// 注册卡牌。如果要写自定义池看添加人物的开头
[RegisterCard(typeof(MzkCardPool))]
public class Around : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		(DynamicVar)new PowerVar<WeakPower>(2m),
        new CardsVar(1)
	};
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
	{
		HoverTipFactory.FromPower<WeakPower>()
	};

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/Around.png";

    public Around() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, ((DynamicVar)((CardModel)this).DynamicVars["WeakPower"]).BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
    }   

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        this.DynamicVars.Cards.UpgradeValueBy(1);
    }
}