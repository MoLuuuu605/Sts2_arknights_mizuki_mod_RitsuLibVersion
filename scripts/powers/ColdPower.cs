using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class ColdPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/ColdPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/ColdPower.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player.Creature || Amount <= 0)
            return;

        Flash();
        await PlayerCmd.LoseEnergy(Amount, player);
        await PowerCmd.Remove<ColdPower>(Owner);
    }
}
