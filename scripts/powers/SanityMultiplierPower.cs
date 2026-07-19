using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
/// 损伤倍率：伤害倍率 * 100 作为层数，计算损伤伤害时用其层数来计算伤害倍率
/// 叠加：依次为 25%(0层) → 40%(15层) → 55%(30层) → 65%(45层) ...
/// </summary>
public sealed class SanityMultiplierPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/Sanity_Buff.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/Sanity_Buff.png";

}
