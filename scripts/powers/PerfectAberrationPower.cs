using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class PerfectAberrationPower : ModPowerTemplate
{
    private const decimal MaxHpGainDamageMultiplier = 3m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/PerfectAberrationPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/PerfectAberrationPower.png";

    public static async Task NotifyMaxHpLoss(PlayerChoiceContext choiceContext, Creature owner, decimal amount, CardModel? source)
    {
        PerfectAberrationPower? power = owner.GetPower<PerfectAberrationPower>();
        if (power == null || amount <= 0)
            return;

        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            owner,
            amount * power.Amount,
            owner,
            source,
            false);

        power.Flash();
    }

    public static async Task NotifyMaxHpGain(PlayerChoiceContext choiceContext, Creature owner, decimal amount, CardModel? source)
    {
        PerfectAberrationPower? power = owner.GetPower<PerfectAberrationPower>();
        if (power == null || amount <= 0 || source == null)
            return;

        decimal damage = amount * MaxHpGainDamageMultiplier * power.Amount;
        await DamageCmd.Attack(damage)
            .TargetingAllOpponents(source.CombatState)
            .Execute(choiceContext);

        power.Flash();
    }
}
