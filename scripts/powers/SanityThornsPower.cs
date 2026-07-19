using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
/// 反伤损伤：玩家回合开始时层数-1，当玩家受到伤害时，给予伤害来源等同于当前层数的 SanityPower
/// </summary>
public sealed class SanityThornsPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/sanity_throns.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/sanity_throns.png";

    /// <summary>
    /// 玩家回合开始时，层数-1
    /// </summary>
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Amount <= 0)
            return;

        // 只处理拥有者是当前玩家的情况
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

    /// <summary>
    /// 当拥有此能力的生物受到伤害后触发，给予伤害来源 SanityPower
    /// </summary>
    /// 
    /// 
    /// 
    /// 
    /// 
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);

        // 只处理自己是受伤者的情况
        if (target != Owner || dealer == Owner)
            return;

        // 伤害来源存在且还活着，且层数 > 0，且实际造成了伤害
        if (dealer != null && dealer.IsAlive && Amount > 0)
        {
            await PowerCmd.Apply<SanityPower>(
                choiceContext,
                dealer,
                Amount,
                Owner,
                cardSource,
                false);

            Flash();
        }
    }
}
