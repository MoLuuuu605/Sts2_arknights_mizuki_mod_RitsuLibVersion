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
public sealed class Purification : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/Purification.png";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Ethereal,
        CardKeyword.Exhaust
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromPower<SanityPower>(),
        HoverTipFactory.FromPower<SanityBurstPower>()
    };
    public Purification() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var player in Owner.Creature.CombatState.Players)
        {
            foreach (var power in player.Creature.Powers.ToList())
            {
                if (power is SanityPower || power is SanityMultiplierPower)
                    await PowerCmd.Remove(power);
            }
        }
    }
}
