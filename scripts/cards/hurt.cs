using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public class Hurt : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型（防御牌是技能类型）
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（Self表示自己）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    public override int MaxUpgradeLevel => 0;

    // 卡牌的基础属性（格挡值）
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new DamageVar(3m,(ValueProp)8)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[2]{
        CardKeyword.Exhaust,
        AutoPlay.Autoplay
    };


    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/hurt.png";
    public Hurt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .Targeting(((CardModel)this).Owner.Creature)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        
    }
}