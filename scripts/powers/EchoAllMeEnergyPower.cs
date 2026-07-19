using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
/// <summary>
/// 回响：众我 — 每打出 10 张牌，获得 1 费。
/// </summary>
public sealed class EchoAllMeEnergyPower : ModPowerTemplate, IPowerExtraIconAmountLabelsProvider
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => true;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/echo_allme.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/echo_allme.png";

    private const int Threshold = 10;

    public IReadOnlyList<ExtraIconAmountLabelSlot> GetPowerExtraIconAmountLabelSlots()
    {
        return new[] { ExtraIconAmountLabelSlot.At(ExtraIconAmountLabelCorner.TopRight, Threshold.ToString()) };
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);
        if (cardPlay.Player.Creature != Owner) return;

        await PowerCmd.Apply<EchoAllMeEnergyPower>(choiceContext, Owner, -1, Owner, null, false);
        if (Amount ==0)
        {
            Flash();
            await PlayerCmd.GainEnergy(1m, cardPlay.Player);
            await PowerCmd.Apply<EchoAllMeEnergyPower>(choiceContext,Owner,Threshold,Owner,null,true);
        }
    }
}
