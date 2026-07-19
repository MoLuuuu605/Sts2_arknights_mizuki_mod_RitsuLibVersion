using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;

namespace Arknights_Mizuki.Scripts.Cards;

// 注册卡牌到 MzkCardPool
[RegisterCard(typeof(MzkCardPool))]
public class MzkDefence : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Defend };
    // 卡牌类型（防御牌是技能类型）
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Basic;
    // 目标类型（Self表示自己）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（格挡值）
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new BlockVar(5m, (ValueProp)8)
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/Defence.png";

    public MzkDefence() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得格挡
    await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
    }

    // 升级后的效果逻辑（5 → 8 格挡）
    protected override void OnUpgrade()
    {
        ((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(3m);
    }
}