using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class BuriedPopulation : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new EnergyVar(1)
    };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1]
    {
        CardKeyword.Exhaust
    };


    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/BuriedPopulation.png";

    public BuriedPopulation() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> skills = PileType.Hand.GetPile(Owner).Cards
            .Where(card => card.Type == CardType.Skill)
            .ToList();

        foreach (CardModel card in skills)
        {
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);
            await PlayerCmd.GainEnergy(1m, Owner);
            await CardPileCmd.Draw(choiceContext, 1, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        this.AddKeyword(CardKeyword.Retain);
    }
}
