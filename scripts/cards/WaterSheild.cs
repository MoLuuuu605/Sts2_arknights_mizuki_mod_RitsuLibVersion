using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.keywords;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class WaterSheild : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new BlockVar(6m,MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new CardsVar(1)
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/WaterSheild.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => (IEnumerable<CardKeyword>)(object)new CardKeyword[1] { AutoPlay.Autoplay };

    public WaterSheild() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block,cardPlay);
        await CardPileCmd.Draw(choiceContext,1,Owner);
    }
    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars.Block.UpgradeValueBy(2m);
    }
}
