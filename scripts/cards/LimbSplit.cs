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
public sealed class LimbSplit : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new BlockVar(16m, (ValueProp)8),
        (DynamicVar)new DynamicVar("MaxHpLoss", 6m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromCard<LimbRecycle>(this.IsUpgraded)
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/LimbSplit.png";

    public LimbSplit() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
        decimal loss = DynamicVars["MaxHpLoss"].BaseValue;
        await CreatureCmd.SetMaxHp(Owner.Creature, Owner.Creature.MaxHp - loss);

        CardModel recycle = CombatState.CreateCard<LimbRecycle>(Owner);
        if (((CardModel)this).IsUpgraded)
            CardCmd.Upgrade(recycle);

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(recycle, PileType.Discard, Owner));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
