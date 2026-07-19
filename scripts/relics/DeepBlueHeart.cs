using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class DeepBlueHeart : ModRelicTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(3m),
        new PowerVar<DexterityPower>(3m),
        new CardsVar(3)
    };

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/Heart.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/Heart.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/Heart.png";

    public override async Task AfterObtained()
    {
        int removableCount = PileType.Deck.GetPile(Owner).Cards.Count(card => card.IsRemovable);
        int maxSelect = Math.Min(DynamicVars.Cards.IntValue, removableCount);
        if (maxSelect <= 0)
        {
            return;
        }

        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 0, maxSelect)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };

        List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(Owner, prefs)).ToList();
        if (cards.Count > 0)
        {
            await CardPileCmd.RemoveFromDeck(cards);
        }
    }

    public override async Task BeforeCombatStart()
    {
        ThrowingPlayerChoiceContext choiceContext = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Strength.BaseValue,
            Owner.Creature,
            null);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Dexterity.BaseValue,
            Owner.Creature,
            null);
    }
    
}
