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
public class NervesRegression : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new PowerVar<WeakPower>(1m),
        new EnergyVar(2),
    };


    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/NervesRegression.png";


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1]
    {
        CardKeyword.Exhaust
    };

    public NervesRegression() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WeakPower>(choiceContext,
        this.Owner.Creature,
        ((DynamicVar)((CardModel)this).DynamicVars["WeakPower"]).BaseValue,
        this.Owner.Creature,
        this
        );
        await PowerCmd.Apply<VulnerablePower>(choiceContext,
        this.Owner.Creature,
        ((DynamicVar)((CardModel)this).DynamicVars["WeakPower"]).BaseValue,
        this.Owner.Creature,
        this
        );
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
    }
    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars.Energy.UpgradeValueBy(1);
    }
}
