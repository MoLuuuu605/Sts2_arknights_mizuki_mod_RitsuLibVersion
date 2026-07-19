using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using Arknights_Mizuki.Scripts.keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Singletons;


[RegisterSingleton]
public class AutoPlayLimit : HookedSingletonModel
{
    private static AutoPlayLimit? _instance;

    private readonly Dictionary<ulong, PlayerAutoPlayCounter> _playedByPlayer = new();

    public AutoPlayLimit() : base(HookedSingletonModel.HookType.Combat)
    {
        _instance = this;
    }

    public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (!card.Keywords.Contains(AutoPlay.Autoplay))
            return;

        PlayerAutoPlayCounter counter = GetOrCreateCounter(card.Owner.NetId);
        counter.PendingCards.Push(card);

        if (counter.DrawDepth > 0)
            return;

        if (counter.IsProcessing)
            return;

        await DrainPendingCards(counter);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        GetOrCreateCounter(cardPlay.Card.Owner.NetId).CardPlayDepth++;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PlayerAutoPlayCounter counter = GetOrCreateCounter(cardPlay.Card.Owner.NetId);
        counter.CardPlayDepth = Math.Max(0, counter.CardPlayDepth - 1);

        if (counter.CardPlayDepth <= 0 && !counter.IsProcessing)
        {
            await ReleaseHeldCards(counter);
        }
    }

    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
    {
        PileType pileType=cardLocation.pileType;
        var position=cardLocation.position;
        
        if (!isAutoPlay || pileType == PileType.None || !card.Keywords.Contains(AutoPlay.Autoplay))
            return cardLocation;

        PlayerAutoPlayCounter counter = GetOrCreateCounter(card.Owner.NetId);
        if (!counter.IsProcessing || counter.ResultCaptureDepth <= 0)
            return cardLocation;

        counter.HeldCards.Add(new HeldAutoPlayCard(card, pileType, position));
        cardLocation.pileType=PileType.Play;
        return cardLocation;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        foreach (PlayerAutoPlayCounter counter in _playedByPlayer.Values)
        {
            await ReleaseHeldCards(counter);
        }
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        foreach (PlayerAutoPlayCounter counter in _playedByPlayer.Values)
        {
            await ReleaseHeldCards(counter);
        }

        _playedByPlayer.Clear();
    }

    private PlayerAutoPlayCounter GetOrCreateCounter(ulong playerId)
    {
        if (!_playedByPlayer.TryGetValue(playerId, out PlayerAutoPlayCounter? counter))
        {
            counter = new PlayerAutoPlayCounter();
            _playedByPlayer[playerId] = counter;
        }

        return counter;
    }

    public static void BeginDrawBatch(Player player)
    {
        if (_instance == null)
            return;

        _instance.GetOrCreateCounter(player.NetId).DrawDepth++;
    }

    public static async Task EndDrawBatch(Player player)
    {
        if (_instance == null)
            return;

        PlayerAutoPlayCounter counter = _instance.GetOrCreateCounter(player.NetId);
        counter.DrawDepth = Math.Max(0, counter.DrawDepth - 1);

        if (counter.DrawDepth <= 0 && !counter.IsProcessing)
        {
            await _instance.DrainPendingCards(counter);
        }
    }

    private async Task DrainPendingCards(PlayerAutoPlayCounter counter)
    {
        if (counter.IsProcessing)
            return;

        counter.IsProcessing = true;
        try
        {
            PlayerChoiceContext choiceContext = new ThrowingPlayerChoiceContext();
            while (counter.PendingCards.Count > 0)
            {
                CardModel pendingCard = counter.PendingCards.Pop();
                if (pendingCard.Pile == null || pendingCard.Owner.Creature.IsDead)
                    continue;

                counter.ResultCaptureDepth++;
                try
                {
                    if (pendingCard.TargetType == TargetType.AnyEnemy)
                    {
                        Creature target = pendingCard.Owner.RunState.Rng.CombatTargets.NextItem(pendingCard.CombatState.HittableEnemies);
                        await CardCmd.AutoPlay(choiceContext, pendingCard, target);
                    }
                    else
                    {
                        await CardCmd.AutoPlay(choiceContext, pendingCard, null);
                    }
                }
                finally
                {
                    counter.ResultCaptureDepth--;
                }
            }
        }
        finally
        {
            counter.IsProcessing = false;
            if (counter.CardPlayDepth <= 0 && counter.DrawDepth <= 0)
            {
                await ReleaseHeldCards(counter);
            }
        }
    }

    private static async Task ReleaseHeldCards(PlayerAutoPlayCounter counter)
    {
        if (counter.HeldCards.Count == 0)
            return;

        List<HeldAutoPlayCard> heldCards = counter.HeldCards.ToList();
        counter.HeldCards.Clear();
        PlayerChoiceContext choiceContext = new ThrowingPlayerChoiceContext();

        foreach (HeldAutoPlayCard heldCard in heldCards)
        {
            if (heldCard.Card.Owner.Creature.IsDead)
                break;

            switch (heldCard.PileType)
            {
                case PileType.Exhaust:
                    await CardCmd.Exhaust(choiceContext, heldCard.Card);
                    break;
                default:
                    await CardPileCmd.Add(heldCard.Card, heldCard.PileType, heldCard.Position);
                    break;
            }
        }
    }

    private sealed class PlayerAutoPlayCounter
    {
        public Stack<CardModel> PendingCards { get; } = new();

        public List<HeldAutoPlayCard> HeldCards { get; } = new();

        public bool IsProcessing { get; set; }

        public int ResultCaptureDepth { get; set; }

        public int CardPlayDepth { get; set; }

        public int DrawDepth { get; set; }
    }

    private readonly record struct HeldAutoPlayCard(CardModel Card, PileType PileType, CardPilePosition Position);
}
