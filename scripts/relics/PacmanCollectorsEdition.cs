using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class PacmanCollectorsEdition : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("ShrinkPower", 1m)
    };

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/pacman_collectors_edition.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/pacman_collectors_edition_outline.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/pacman_collectors_edition.png";
}
