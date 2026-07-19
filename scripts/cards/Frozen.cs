using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Frozen : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = true;

    public override int MaxUpgradeLevel => 0;

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/Frozen.png";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Exhaust
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromPower<ColdPower>()
    };
    public override bool HasTurnEndInHandEffect => true;

    public Frozen() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<ColdPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this, false);
    }
}
