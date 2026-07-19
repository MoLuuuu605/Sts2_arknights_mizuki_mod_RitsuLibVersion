using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.keywords;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class BottomFeed : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new CardsVar(2)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromKeyword(Tandi.tandi)
    };


    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/BottomFeed.png";

    public BottomFeed() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardPile drawPile = PileType.Draw.GetPile(Owner);
        List<CardModel> bottomCards = drawPile.Cards
            .Skip(Math.Max(0, drawPile.Cards.Count - DynamicVars.Cards.IntValue))
            .ToList();

        if (bottomCards.Count == 0)
            return;

        CardModel picked = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            bottomCards,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
        if (picked != null)
        {
            int Energy = Owner.PlayerCombatState.Energy;
            if(picked.TargetType == TargetType.AnyEnemy)
            {
                Creature target = picked.Owner.RunState.Rng.CombatTargets.NextItem(picked.CombatState.HittableEnemies);
                await CardCmd.AutoPlay(choiceContext, picked, target);
            }
            else await CardCmd.AutoPlay(choiceContext,picked,null);
        
        // 未选中的牌全部弃掉
        foreach (var card in bottomCards)
        {
            if (card != picked)
                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Discard));
                
        }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2m);
    }
}
