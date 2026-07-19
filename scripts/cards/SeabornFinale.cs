using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class SeabornFinale : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new DamageVar(0m, ValueProp.Move),
        (DynamicVar)new IntVar("Hits", 2m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromCard<BabyHs>()
    };


    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/SeabornFinale.png";

    public SeabornFinale() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> babies = PileType.Draw.GetPile(Owner).Cards
            .Concat(PileType.Exhaust.GetPile(Owner).Cards)
            .Where(card => card is BabyHs)
            .ToList();

        decimal damage = babies.Count;

        for (int i = 0; i < DynamicVars["Hits"].IntValue; i++)
        {
            await DamageCmd.Attack(damage)
                .FromCard(this,cardPlay)
                .TargetingAllOpponents(CombatState)
                .Execute(choiceContext);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["Hits"].UpgradeValueBy(1m);
    }
}
