using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class KeyCharge : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;
        public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(1)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/KeyCharge.png";

    public KeyCharge() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Owner.GetRelic<Key>()?.AddCharges(1);

        if (DeckVersion != null && DeckVersion.Pile?.Type == PileType.Deck)
        {
            await CardPileCmd.RemoveFromDeck(DeckVersion);
        }
        else if (Pile?.Type == PileType.Deck)
        {
            await CardPileCmd.RemoveFromDeck(this);
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
    }
}