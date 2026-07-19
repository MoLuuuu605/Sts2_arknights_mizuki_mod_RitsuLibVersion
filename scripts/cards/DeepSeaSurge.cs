using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.keywords;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class DeepSeaSurge : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new CardsVar(3),
        (DynamicVar)new BlockVar(6m, ValueProp.Move)
    };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(Tandi.tandi)
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/DeepSeaSurge.png";

    public DeepSeaSurge() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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
        // 未选中的牌全部弃掉
        int energy = 0;
        foreach (var card in bottomCards)
        {
            if (card != picked)
                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Discard));
                energy += card.EnergyCost.GetResolved();   
        }
        energy -= picked.EnergyCost.GetResolved();
        energy *= 3 ;
        if(energy > 0)
        await CreatureCmd.GainBlock(Owner.Creature, energy,ValueProp.Move, cardPlay, false);
        await CardPileCmd.Add(picked, PileType.Draw,CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2m);
    }
}
