using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Shadow : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/ShadowReturn.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new PowerVar<ShadowEvolutionPower>(1m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IReadOnlyList<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromCard<BabyHs>()
    };

    public Shadow()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay _)
    {
        await PowerCmd.Apply<ShadowEvolutionPower>(
            choiceContext,
            ((CardModel)this).Owner.Creature,
            ((CardModel)this).DynamicVars["ShadowEvolutionPower"].BaseValue,
            ((CardModel)this).Owner.Creature,
            (CardModel)(object)this,
            false
        );
    }

    protected override void OnUpgrade()
    {
        ((CardModel)this).AddKeyword(CardKeyword.Innate);
    }
}
