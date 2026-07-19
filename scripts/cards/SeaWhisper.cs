using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class SeaWhisper : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
    {
        (DynamicVar)new PowerVar<StrengthPower>(2m),
        new PowerVar<DexterityPower>(2m),
        new PowerVar<ArtifactPower>(2m)
    };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Exhaust  // 
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
      HoverTipFactory.FromPower<StrengthPower>(),
      HoverTipFactory.FromPower<DexterityPower>(),
    HoverTipFactory.FromPower<ArtifactPower>()  
    ];

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/SeaWhisper.png";


    public SeaWhisper() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext,
        this.Owner.Creature,
        -((DynamicVar)((CardModel)this).DynamicVars["StrengthPower"]).BaseValue,
        this.Owner.Creature,
        this
        );
        await PowerCmd.Apply<DexterityPower>(choiceContext,
        this.Owner.Creature,
        -((DynamicVar)((CardModel)this).DynamicVars["DexterityPower"]).BaseValue,
        this.Owner.Creature,
        this
        );
        await PowerCmd.Apply<ArtifactPower>(choiceContext,
        this.Owner.Creature,
        ((DynamicVar)((CardModel)this).DynamicVars["ArtifactPower"]).BaseValue,
        this.Owner.Creature,
        this
        );
    }
    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars["StrengthPower"].UpgradeValueBy(-1);
        this.DynamicVars["DexterityPower"].UpgradeValueBy(-1);
    }
}