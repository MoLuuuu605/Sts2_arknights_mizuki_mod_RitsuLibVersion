using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.StatusSlots;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Arknights_Mizuki.Scripts.Enchantments;


[RegisterEnchantment]
public sealed class RevelationEnchantment : ModEnchantmentTemplate
{

    private bool used=false;
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2),
        new DynamicVar("Hp", 4m),
        new PowerVar<SanityPower>(4m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[2]
    {
        HoverTipFactory.FromPower<SanityPower>(),
HoverTipFactory.FromPower<SanityBurstDescriptionPower>()
    };

    public override string? CustomIconPath => "res://Arknights_Mizuki/images/map/ancients/last_tidewatcher.png";

    public override bool HasExtraCardText => true;

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (!StatusSlotManager.IsSlotEnabled(StatusSlotType.Revelation))
            return;

        if(used)return;
        int roll = Card.Owner.RunState.Rng.Niche.NextInt(10);
        if (roll < 3)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Card.Owner);
            return;
        }

        if (roll < 6)
        {
            await CreatureCmd.Heal(Card.Owner.Creature, DynamicVars["Hp"].BaseValue);
            return;
        }

        if (roll < 9)
        {

            Creature target = Card.Owner.RunState.Rng.CombatTargets.NextItem(Card.CombatState.HittableEnemies);
            await PowerCmd.Apply<SanityPower>(
                choiceContext,
                target,
                DynamicVars["SanityPower"].BaseValue,
                Card.Owner.Creature,
                Card);
            return;
        }
        
        await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(Card.Owner).ToMutable(), Card.Owner);
        used = true;
    }
}
