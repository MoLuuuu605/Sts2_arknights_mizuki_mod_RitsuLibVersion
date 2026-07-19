using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class HatchingInfusion : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new CardsVar(4)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromCard<BabyHs>(this.IsUpgraded)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => (IEnumerable<CardKeyword>)(object)new CardKeyword[1]
    {
        CardKeyword.Exhaust
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/HatchingInfusion.png";

    public HatchingInfusion() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var players = Owner.Creature.CombatState.Players.ToList();
        int count = DynamicVars.Cards.IntValue;
        var combatState = Owner.Creature.CombatState;

        foreach (var player in players)
        {
            if (player.Creature.IsDead) continue;

            for (int i = 0; i < count; i++)
            {
                CardModel baby = combatState.CreateCard<BabyHs>(player);
                if (IsUpgraded)
                    CardCmd.Upgrade(baby);
                    
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(baby, PileType.Draw, Owner, CardPilePosition.Bottom));
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
