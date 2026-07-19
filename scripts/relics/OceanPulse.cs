using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class OceanPulse : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(4m, ValueProp.Unpowered)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Block)
    };

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/pulse.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/pulse.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/pulse.png";

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
        {
            return;
        }

        if (cardPlay.Card.Pool is not ColorlessCardPool && cardPlay.Card.Pool is not TokenCardPool)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null, fast: true);
    }
}
