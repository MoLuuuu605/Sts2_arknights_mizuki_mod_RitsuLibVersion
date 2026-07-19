using Arknights_Mizuki.Scripts.StatusSlots;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Arknights_Mizuki.Scripts.Powers;

[RegisterPower]
public sealed class VictoriaGloryPower : ModPowerTemplate
{
    private int _damageEventsThisCombat;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath =>
        "res://Arknights_Mizuki/images/powers/victoria_glory.png";

    public override string CustomBigIconPath => CustomIconPath;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Amount <= 0 || amount <= 0 || !ReferenceEquals(target, Owner) || Owner.Player == null)
            return 1m;

        return StableDamageRoll(Owner.Player, amount, props, dealer, cardSource, cardPlay) < 25
            ? 0m
            : 1m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
        if (ReferenceEquals(target, Owner) && result.TotalDamage > 0)
            _damageEventsThisCombat++;
    }

    private int StableDamageRoll(
        Player targetPlayer,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        ICombatState? combatState = Owner.CombatState;
        unchecked
        {
            uint hash = 2166136261u;
            hash = HashInt(hash, StatusSlotManager.GetRunState()?.CurrentActIndex ?? 0);
            hash = HashInt(hash, StatusSlotManager.GetRunState()?.TotalFloor ?? 0);
            hash = HashInt(hash, combatState?.RoundNumber ?? 0);
            hash = HashInt(hash, targetPlayer.PlayerCombatState.TurnNumber);
            hash = HashInt(hash, _damageEventsThisCombat);
            hash = HashInt(hash, (int)targetPlayer.NetId);
            hash = HashInt(hash, GetCreatureIndex(Owner, combatState));
            hash = HashInt(hash, GetCreatureIndex(dealer, combatState));
            hash = HashInt(hash, (int)amount);
            hash = HashInt(hash, (int)props);
            hash = HashInt(hash, cardPlay?.PlayIndex ?? -1);
            hash = HashString(hash, cardSource?.GetType().FullName ?? "");
            return (int)(hash % 100u);
        }
    }

    private static uint HashInt(uint hash, int value)
    {
        unchecked
        {
            hash ^= (uint)value;
            return hash * 16777619u;
        }
    }

    private static uint HashString(uint hash, string value)
    {
        unchecked
        {
            foreach (char character in value)
            {
                hash ^= character;
                hash *= 16777619u;
            }
            return hash;
        }
    }

    private static int GetCreatureIndex(Creature? creature, ICombatState? combatState)
    {
        if (creature == null || combatState == null)
            return -1;

        int allyIndex = IndexOf(combatState.Allies, creature);
        if (allyIndex >= 0)
            return allyIndex;

        int enemyIndex = IndexOf(combatState.Enemies, creature);
        return enemyIndex >= 0 ? 1000 + enemyIndex : -1;
    }

    private static int IndexOf(IReadOnlyList<Creature> creatures, Creature creature)
    {
        for (int i = 0; i < creatures.Count; i++)
            if (ReferenceEquals(creatures[i], creature))
                return i;
        return -1;
    }
}
