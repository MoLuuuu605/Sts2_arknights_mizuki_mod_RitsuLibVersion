using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class FleshAberration : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new PowerVar<StrengthPower>(2m),
        new PowerVar<VulnerablePower>(1m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/FleshAberration.png";


    public FleshAberration() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext,
        this.Owner.Creature,
        ((DynamicVar)((CardModel)this).DynamicVars["StrengthPower"]).BaseValue,
        this.Owner.Creature,
        this
        );
        await PowerCmd.Apply<VulnerablePower>(choiceContext,
        this.Owner.Creature,
        ((DynamicVar)((CardModel)this).DynamicVars["VulnerablePower"]).BaseValue,
        this.Owner.Creature,
        this
        );
    }
    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars["StrengthPower"].UpgradeValueBy(1);
    }
}