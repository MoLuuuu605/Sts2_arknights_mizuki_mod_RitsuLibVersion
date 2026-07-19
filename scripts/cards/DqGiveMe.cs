using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class DqGiveMe : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
    {
        (DynamicVar)new BlockVar(6m, (ValueProp)8),
        (DynamicVar)new DynamicVar("DrawPileThreshold", 10m),
        (DynamicVar)new IntVar("Picks", 1m)
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/DqGiveMe.png";

    public DqGiveMe() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);

        CardPile drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile.Cards.Count < DynamicVars["DrawPileThreshold"].BaseValue)
            return;

        int pickCount = Math.Min(DynamicVars["Picks"].IntValue, drawPile.Cards.Count);
        List<CardModel> pickedCards = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            drawPile.Cards,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, pickCount)
        )).ToList();

        foreach (CardModel pickedCard in pickedCards)
        {
            await CardPileCmd.Add(pickedCard,PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Picks"].UpgradeValueBy(1m);
    }
}

