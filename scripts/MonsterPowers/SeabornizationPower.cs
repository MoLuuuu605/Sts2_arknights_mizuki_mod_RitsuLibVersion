using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
///海嗣化，每若干层提高输出
/// </summary>
public sealed class SeabornizationPower: ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/SeabornizationPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/SeabornizationPower.png";
}
