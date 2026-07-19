using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Actions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
public sealed class HarvestAttackPower : ModPowerTemplate
{
    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/HarvestAttackPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/HarvestAttackPower.png";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {

        if(side != Owner.Side || !Owner.IsAlive)return;
        await PowerCmd.Remove<HarvestAttackAction>(Owner);
        await PowerCmd.Apply<HarvestAttackAction>(choiceContext,Owner,1,Owner,null);
    }
}
