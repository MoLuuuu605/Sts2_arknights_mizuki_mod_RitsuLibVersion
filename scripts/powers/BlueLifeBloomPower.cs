using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class BlueLifeBloomPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    public override bool ShouldReceiveCombatHooks => true;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/BlueLifeBloomPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/BlueLifeBloomPower.png";

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Amount <= 0)
            return;

        await CreatureCmd.GainMaxHp(Owner, Amount);
        Flash();
    }
}
