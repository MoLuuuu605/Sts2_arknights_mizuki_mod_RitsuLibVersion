using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Sign : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
    {
        (DynamicVar)new PowerVar<SanityBuffPower>(1m),
        (DynamicVar)new DamageVar(3m, ValueProp.Unblockable),
        new PowerVar<VigorPower>(3m)
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/Sign.png";


    public Sign() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SanityBuffPower>(choiceContext,
        this.Owner.Creature,
        ((DynamicVar)((CardModel)this).DynamicVars["SanityBuffPower"]).BaseValue,
        this.Owner.Creature,
        this
        );
        await PowerCmd.Apply<VigorPower>(choiceContext,
        this.Owner.Creature,
        ((DynamicVar)((CardModel)this).DynamicVars["VigorPower"]).BaseValue,
        this.Owner.Creature,
        this
        );
        await CreatureCmd.Damage(
            choiceContext,
            ((CardModel)this).Owner.Creature,
            DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            ((CardModel)this).Owner.Creature);
    }
    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars["SanityBuffPower"].UpgradeValueBy(1);
        ((CardModel)this).DynamicVars.Damage.UpgradeValueBy(-1);
    }
}