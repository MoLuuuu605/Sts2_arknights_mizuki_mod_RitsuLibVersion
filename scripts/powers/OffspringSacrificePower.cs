using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class OffspringSacrificePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/IzumikEvolutionPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/IzumikEvolutionPower.png";
}
