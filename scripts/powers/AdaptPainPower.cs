using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
/// <summary>
/// 痛感适应：每失去 4 点生命，获得 1 层缓冲
/// </summary>
public sealed class AdaptPainPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    private const int DamagePerBuffer = 4;  // 每 4 点伤害获得 1 层缓冲
    private int _accumulatedDamageTimes = 0;     // 累计受到的伤害值

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
        
        if (target != Owner)
            return;
        if(result.WasFullyBlocked)return;
        
        // 累计伤害值
        _accumulatedDamageTimes += 1;
        
        // 检查是否达到阈值
        if (_accumulatedDamageTimes >= DamagePerBuffer)
        {
            _accumulatedDamageTimes = 0;
            await PowerCmd.Apply<BufferPower>(
                choiceContext,
                Owner,
                1,
                Owner,
                cardSource,
                false);
            
            Flash();
        }
    }
}