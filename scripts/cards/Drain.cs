using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class Drain : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new EnergyVar(2)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[2]
    {
        HoverTipFactory.FromPower<DrawCardsNextTurnPower>(),
        HoverTipFactory.FromPower<EnergyNextTurnPower>()
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1]
    {
        CardKeyword.Exhaust
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/Drain.png";

    public Drain() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> cardsToExhaust = PileType.Hand.GetPile(Owner).Cards.ToList();
        int cardCount = cardsToExhaust.Count;

        foreach (CardModel card in cardsToExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        if (cardCount > 0)
        {
            await PowerCmd.Apply<DrawCardsNextTurnPower>(
                choiceContext,
                Owner.Creature,
                cardCount,
                Owner.Creature,
                (CardModel)(object)this);
        }

        await PowerCmd.Apply<EnergyNextTurnPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Energy.BaseValue,
            Owner.Creature,
            (CardModel)(object)this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
