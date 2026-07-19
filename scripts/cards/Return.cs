using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Return : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new CardsVar(4),
        (DynamicVar)new DamageVar(4m, ValueProp.Unblockable),
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/Return.png";


    public Return() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext,((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue,Owner,false);
        await CreatureCmd.Damage(choiceContext,Owner.Creature,DynamicVars.Damage,Owner.Creature);
    }
    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars.Damage.UpgradeValueBy(-2);
    }
}