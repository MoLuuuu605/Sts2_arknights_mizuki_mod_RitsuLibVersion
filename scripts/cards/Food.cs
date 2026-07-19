using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using Arknights_Mizuki.Scripts.Pools;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Food : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;  // 蓝色
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new EnergyVar(1),   // 获得 1 点能量（升级不变）
        (DynamicVar)new CardsVar(4)      // 抽 2 张牌，升级后抽 3 张
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/Food.png";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Exhaust  // 消耗
    };

    public Food() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 1 点能量
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
        
        // 抽 2 张牌（升级后抽 3 张）
        int drawAmount = (int)base.DynamicVars.Cards.BaseValue;
        await CreatureCmd.Heal(Owner.Creature,DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
        // 升级：抽牌 2 → 3
        base.DynamicVars.Cards.UpgradeValueBy(2);
    }
}