using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.keywords;
using Arknights_Mizuki.Scripts.Powers;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Trauma : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new PowerVar<SanityBuffPower>(2m)
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/Trauma.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => (IEnumerable<CardKeyword>)(object)new CardKeyword[1] { AutoPlay.Autoplay };

    public Trauma() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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
    }
    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars["SanityBuffPower"].UpgradeValueBy(1);
    }
}
