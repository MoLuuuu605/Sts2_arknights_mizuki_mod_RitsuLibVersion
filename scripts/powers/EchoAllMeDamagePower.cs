using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Cards;
using Arknights_Mizuki.Scripts.StatusSlots;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
/// <summary>
/// 回响：众我 — 每打出 5 张牌，对所有敌人造成 10 点伤害。
/// </summary>
public sealed class EchoAllMeDamagePower : ModPowerTemplate, IPowerExtraIconAmountLabelsProvider
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => true;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/echo_allme.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/echo_allme.png";

    private const int Threshold = 5;
    private const int DamageAmount = 10;

    public IReadOnlyList<ExtraIconAmountLabelSlot> GetPowerExtraIconAmountLabelSlots()
    {
        return new[] { ExtraIconAmountLabelSlot.At(ExtraIconAmountLabelCorner.TopRight, Threshold.ToString()) };
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);
        if (cardPlay.Player.Creature != Owner) return;
        if (!StatusSlotManager.ShouldSlotAffectCreature(StatusSlotType.SwarmCall, Owner)) return;

        await PowerCmd.Apply<EchoAllMeDamagePower>(choiceContext, Owner, -1, Owner, null, false);

        if (Amount == 0)
        {
            Flash();
            var cs = (Owner).CombatState;
            var ctx = new ThrowingPlayerChoiceContext();
            foreach (var enemy in cs.Enemies)
                if (enemy.IsAlive)
                    await CreatureCmd.Damage(ctx, enemy, DamageAmount,
                        ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            await PowerCmd.Apply<EchoAllMeDamagePower>(choiceContext,Owner,Threshold,Owner,null,true);
        }

    }
}
