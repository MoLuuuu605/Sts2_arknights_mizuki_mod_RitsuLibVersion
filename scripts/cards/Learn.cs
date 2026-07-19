using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.keywords;

namespace Arknights_Mizuki.Scripts.Cards;

// 注册卡牌到 MzkCardPool
[RegisterCard(typeof(MzkCardPool))]
public class Learn : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型（防御牌是技能类型）
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（Self表示自己）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（格挡值）
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new CardsVar(2)
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/learn.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [AutoPlay.Autoplay,CardKeyword.Exhaust];
    public Learn() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
    }

    protected override void OnUpgrade()
    {
        ((DynamicVar)((CardModel)this).DynamicVars.Cards).UpgradeValueBy(1);
    }
}
