using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Enemies;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class IzumikPhaseShiftPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/IzumikEvolutionPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/IzumikEvolutionPower.png";

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || result.UnblockedDamage <= 0 || Owner.Monster is not Izumik izumik)
            return;

        if (Owner.CurrentHp > Amount)
            return;

        Flash();
        izumik.QueuePhaseTwoIntent();
        await PowerCmd.Remove(this);
    }
}
