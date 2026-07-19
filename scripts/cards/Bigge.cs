using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Bigge : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = true;

    // 参考官方写法：使用 CardsVar 或者 IntVar
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new CardsVar(2)
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/bigge.png";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[2]
    {
        HoverTipFactory.FromPower<SanityPower>(),
        HoverTipFactory.FromPower<SanityBurstDescriptionPower>()
    };
    public Bigge() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取最多可消耗数量
        int maxPicks = base.DynamicVars.Cards.IntValue;
        
        if (maxPicks < 1)
            return;
        
        // 获取弃牌堆
        CardPile discardPile = PileType.Discard.GetPile(base.Owner);
        
        if (discardPile.Cards.Count == 0)
            return;
        
        // 实际可消耗数量
        int pickCount = System.Math.Min(maxPicks, discardPile.Cards.Count);
        var Count=0;
        // 🔧 修改点：使用 FromSimpleGrid 替代 FromCombatPile
		if (pickCount > 0)
		{
			var selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, PileType.Discard.GetPile(base.Owner).Cards, base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, 0, pickCount));
        foreach (var card in selectedCards)
        {
            if (card != null)
            {
                // 将选中的牌从弃牌堆中移除
                await CardCmd.Exhaust(choiceContext,card);
                Count++;
            }
        }
        }
        int consumedCount = Count;
        
        // 对所有敌人施加等同于消耗牌数量的 SanityPower
        var opponents = ((CardModel)this).CombatState.GetOpponentsOf(Owner.Creature).ToList();
        foreach (var enemy in opponents)
        {
            if (enemy != null && enemy.IsAlive)
            {
                await PowerCmd.Apply<SanityPower>(
                    choiceContext,
                    enemy,
                    consumedCount,
                    base.Owner.Creature,
                    this,
                    false
                );
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：从 1 变成 3
        base.DynamicVars.Cards.UpgradeValueBy(2m);
    }
}
