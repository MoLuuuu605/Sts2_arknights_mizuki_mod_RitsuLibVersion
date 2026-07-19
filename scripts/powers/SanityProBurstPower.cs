using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
/// 损伤爆发，当爆发损伤时获得一层反移情;
/// </summary>
public sealed class SanityProBurstPower: ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/Sanity.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/Sanity.png";
}