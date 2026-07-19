using Arknights_Mizuki.Scripts.StatusSlots;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
public sealed class FocusDisorderPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath =>
        "res://Arknights_Mizuki/images/powers/focus_disorder.png";

    public override string CustomBigIconPath => CustomIconPath;

    public override async Task AfterAutoPrePlayPhaseEnteredLate(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        await base.AfterAutoPrePlayPhaseEnteredLate(choiceContext, player);
        if (!ReferenceEquals(player.Creature, Owner) || player.PlayerCombatState.TurnNumber > 1)
            return;

        ICombatState combatState = Owner.CombatState;
        using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
        {
            int startTurn = player.PlayerCombatState.TurnNumber;
            for (int cardsPlayed = 0; cardsPlayed < 13; cardsPlayed++)
            {
                if (CombatManager.Instance.IsOverOrEnding ||
                    CombatManager.Instance.IsPlayerReadyToEndTurn(player) ||
                    player.PlayerCombatState.TurnNumber != startTurn)
                {
                    break;
                }

                CardModel? card = null;
                Creature? target = null;
                foreach (CardModel candidate in PileType.Hand.GetPile(player).Cards)
                {
                    if (!candidate.CanPlay() || !TryGetTarget(candidate, combatState, player, out target))
                        continue;

                    card = candidate;
                    break;
                }

                if (card == null)
                    break;

                await card.SpendResources();
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
            }
        }
    }

    private static bool TryGetTarget(
        CardModel card,
        ICombatState combatState,
        Player player,
        out Creature? target)
    {
        target = null;
        switch (card.TargetType)
        {
            case TargetType.AnyEnemy:
                target = combatState.HittableEnemies.FirstOrDefault();
                return target != null;
            case TargetType.AnyAlly:
                List<Creature> allies = combatState.Allies
                    .Where(creature => creature.IsAlive && creature.IsPlayer && creature != player.Creature)
                    .ToList();
                if (allies.Count == 0)
                    return false;
                target = player.RunState.Rng.CombatTargets.NextItem(allies);
                return true;
            case TargetType.AnyPlayer:
                target = player.Creature;
                return true;
            default:
                return true;
        }
    }
}
