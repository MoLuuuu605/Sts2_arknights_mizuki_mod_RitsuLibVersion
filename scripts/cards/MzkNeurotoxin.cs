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

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class MzkNeurotoxin : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 2;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（None 表示无需手动选择目标）
    private const TargetType targetType = TargetType.None;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 每次给予的损伤层数
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new PowerVar<SanityPower>(2m),
        (DynamicVar)new DynamicVar("Times",3m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[2]
    {
        HoverTipFactory.FromPower<SanityPower>(),
        HoverTipFactory.FromPower<SanityBurstDescriptionPower>()
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/neurotoxin.png";

    public MzkNeurotoxin() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        for (int i = 0; i < this.DynamicVars["Times"].BaseValue; i++)
        {
            var target = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
            await PowerCmd.Apply<SanityPower>(
                choiceContext,
                target,
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
        DynamicVars["SanityPower"].UpgradeValueBy(1);
    }
}