using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class Determination : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(3)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Slimed>();

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/determination.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/determination.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/determination.png";

    public override async Task AfterObtained()
    {
        if (Owner.RunState is MegaCrit.Sts2.Core.Runs.RunState runState)
        {
            ActUnlockHelper.EnsureFourthActUnlocked(runState);
        }

        List<CardModel> curses = ModelDb.AllCards
            .Where(card => card.Type == CardType.Curse && card.CanBeGeneratedByModifiers)
            .ToList()
            .StableShuffle(Owner.RunState.Rng.Niche)
            .Take(DynamicVars.Cards.IntValue)
            .ToList();

        if (curses.Count == 0)
        { 
            curses.Add(ModelDb.Card<Doubt>());
        }

        await CardPileCmd.AddCursesToDeck(curses, Owner);
    }

    public override async Task BeforeCombatStart()
    {
        CardModel slimed = Owner.Creature.CombatState.CreateCard<Slimed>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(slimed, PileType.Hand, Owner);
    }
}
