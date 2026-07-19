using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class Observation : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(4m, ValueProp.Unpowered)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Block)
    };

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/watching.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/watching.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/watching.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
    }
}
