using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.keywords;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Share : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new DamageVar(6m, (ValueProp)6)
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/share.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [AutoPlay.Autoplay];

    public Share() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .TargetingRandomOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        ((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
    }
}
