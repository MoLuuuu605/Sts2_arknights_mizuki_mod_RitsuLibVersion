using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Targeting;
namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class RecycleCommand : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const bool shouldShowInCardLibrary = true;


    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new CardsVar(1)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromPower<SeabornizationPower>()
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1] { CardKeyword.Exhaust };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/RecycleCommand.png";

    public RecycleCommand() : base(energyCost, type, rarity, MinionTargetTypes.AnyMinion, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? minion =cardPlay.Target;
        if (minion == null)
            return;

        int stacks = minion.GetPowerAmount<SeabornizationPower>();
        await CreatureCmd.Heal(minion,9999);

        if (stacks > 0)
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(stacks, ValueProp.Move), cardPlay, false);

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
