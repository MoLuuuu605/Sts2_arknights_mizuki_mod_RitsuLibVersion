using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Cards;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class BuoyantVesicle : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Float", 1m)
    };

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/buoyant_vesicle.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/buoyant_vesicle.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/buoyant_vesicle.png";

    public override async Task BeforeCombatStart()
    {
        Flash();
        await GroupHatching.Float(new ThrowingPlayerChoiceContext(), ModelDb.Card<Float>().ToMutable(), Owner, DynamicVars["Float"].BaseValue);
    }
}
