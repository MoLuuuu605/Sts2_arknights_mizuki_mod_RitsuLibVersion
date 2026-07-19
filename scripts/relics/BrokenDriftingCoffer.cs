using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class BrokenDriftingCoffer : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new GoldVar(30)
    };

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/broken_drifting_box.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/drifting_coffer_outline.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/broken_drifting_box.png";

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
        return;
    }
}
