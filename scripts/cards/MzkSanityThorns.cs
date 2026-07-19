using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class MzkSanityThorns : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new PowerVar<SanityThornsPower>(2m),
        (DynamicVar)new BlockVar(10m, (ValueProp)8)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromPower<SanityThornsPower>(),
        HoverTipFactory.FromPower<SanityPower>(),
        HoverTipFactory.FromPower<SanityBurstDescriptionPower>()
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/sanity_thorns.png";

    public MzkSanityThorns() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SanityThornsPower>(
            choiceContext,
            ((CardModel)this).Owner.Creature,
            ((DynamicVar)((CardModel)this).DynamicVars["SanityThornsPower"]).BaseValue,
            ((CardModel)this).Owner.Creature,
            (CardModel)(object)this,
            false
        );
        await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
    }

    protected override void OnUpgrade()
    {
        ((DynamicVar)((CardModel)this).DynamicVars["SanityThornsPower"]).UpgradeValueBy(1m);
        ((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(2m);

    }
}