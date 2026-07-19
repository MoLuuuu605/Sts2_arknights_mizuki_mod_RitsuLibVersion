using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
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
public sealed class BottledSanity : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AnyEnemy;

    public override string? CustomImagePath => "res://Arknights_Mizuki/images/potions/BottledSanity.png";

    public override string? CustomOutlinePath => CustomImagePath;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<SanityPower>(4m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<SanityPower>(),
HoverTipFactory.FromPower<SanityBurstDescriptionPower>()
    };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);

        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("7357ff"));

        await PowerCmd.Apply<SanityPower>(
            choiceContext,
            target,
            DynamicVars["SanityPower"].BaseValue,
            Owner.Creature,
            null);
    }
}
