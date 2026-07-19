using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class Bed : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new DynamicVar("CardsPerEnergy", 3m),
        (DynamicVar)new BlockVar(0m, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromCard<BabyHs>()
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/Bed.png";

    public Bed() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int energy = ResolveEnergyXValue();
        int cardsToAdd = energy * DynamicVars["CardsPerEnergy"].IntValue;

        for (int i = 0; i < cardsToAdd; i++)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                CombatState.CreateCard<BabyHs>(Owner),
                PileType.Draw,
                Owner,
                CardPilePosition.Bottom));
        }

        int babyHsCount =
            PileType.Draw.GetPile(Owner).Cards.Count(card => card is BabyHs) +
            PileType.Discard.GetPile(Owner).Cards.Count(card => card is BabyHs);

        if (babyHsCount > 0)
        {
            await CreatureCmd.GainBlock(
                Owner.Creature,
                new BlockVar(babyHsCount, ValueProp.Move),
                cardPlay,
                false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CardsPerEnergy"].UpgradeValueBy(1m);
    }
}
