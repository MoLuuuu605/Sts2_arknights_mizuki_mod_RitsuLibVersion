using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class FroKid : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new DamageVar(3m, (ValueProp)8),
        (DynamicVar)new IntVar("Hits", 2m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromCard<BabyHs>()
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/FroKid.png";

    public FroKid() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        AttackCommand command = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars["Hits"].IntValue)
            .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target);

        await command.Execute(choiceContext);

        int unblockedHits = command.Results
            .SelectMany(hitResults => hitResults)
            .Count(result => !result.WasFullyBlocked);

        for (int i = 0; i < unblockedHits; i++)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                CombatState.CreateCard<BabyHs>(Owner),
                PileType.Draw,
                Owner,
                CardPilePosition.Bottom));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Hits"].UpgradeValueBy(1m);
    }
}
