using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Cards;
using Arknights_Mizuki.Scripts.Pools;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Arknights_Mizuki.Scripts.Potions;

[RegisterPotion(typeof(MzkPotionPool))]
public sealed class BottledSeaborn : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override string? CustomImagePath => "res://Arknights_Mizuki/images/potions/BottledSeaborn.png";

    public override string? CustomOutlinePath => CustomImagePath;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(4)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<BabyHs>()
    };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        NCombatRoom.Instance?.PlaySplashVfx(Owner.Creature, new Color("29b8ff"));

        List<CardModel> cards = new();
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var BabyHs = Owner.Creature.CombatState.CreateCard<BabyHs>(Owner);
            CardCmd.Upgrade(BabyHs);
            cards.Add(BabyHs);
        }

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(
            cards,
            PileType.Draw,
            Owner,
            CardPilePosition.Bottom));
    }
}
