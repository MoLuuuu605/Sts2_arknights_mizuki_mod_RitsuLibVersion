using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
/// 潜行：本回合受到的伤害减少30%（可叠层），每回合开始层数-1
/// </summary>
public sealed class StealthPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/stealth.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/stealth_big.png";

    private const decimal DamageReductionPerStack = 0.3m;

    /// <summary>
    /// 修改伤害倍率，每层减少30%伤害
    /// </summary>
    /// 
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {

        if (target != Owner)
            return 1m;

        if (Amount <= 0)
            return 1m;

        if (!props.IsPoweredAttack())
            return 1m;

        // 每层减伤30%，倍率 = 1 - 0.3 * 层数，最低降到0
        decimal multiplier = 0.7m;
        if (multiplier < 0m)
            multiplier = 0m;

        return multiplier;
    }

    /// <summary>
    /// 玩家回合开始时，层数-1
    /// </summary>
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Amount <= 0)
            return;

        if (Owner != player.Creature)
            return;

        await PowerCmd.ModifyAmount(
            choiceContext,
            this,
            -1,
            null,
            null,
            false);

        Flash();
    }
}