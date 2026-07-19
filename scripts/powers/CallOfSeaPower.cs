using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Actions;
using Arknights_Mizuki.Scripts.Cards;
using Arknights_Mizuki.Scripts.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class CallOfSeaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/CallOfSeaPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/CallOfSeaPower.png";

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        if (cardPlay.Card.Owner != Owner.Player)
            return;

        if (cardPlay.Card is CallOfSea)
            return;

        Creature? floatingSea = Owner.Player.PlayerCombatState?.Pets.FirstOrDefault(p =>
            p is { IsAlive: true, IsPet: true, Monster: FloatingSeaMinion });
        if (floatingSea != null)
        {
            await PowerCmd.Apply<FloatingSeaBlockAction>(choiceContext, floatingSea, 1m, floatingSea, cardPlay.Card);
        }

        Creature? harvest = Owner.Player.PlayerCombatState?.Pets.FirstOrDefault(p =>
            p is { IsAlive: true, IsPet: true, Monster: HarvestMinion });
        if (harvest != null)
        {
            await PowerCmd.Apply<HarvestAttackAction>(choiceContext, harvest, 1m, harvest, cardPlay.Card);
        }

        Flash();
        await PowerCmd.Remove<CallOfSeaPower>(Owner);
    }
}
