using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class BedCollapse : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new CardsVar(6)
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/BedCollapse.png";

    public BedCollapse() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardPile drawPile = PileType.Draw.GetPile(Owner);
        List<CardModel> bottomCards = drawPile.Cards
            .Skip(Math.Max(0, drawPile.Cards.Count - DynamicVars.Cards.IntValue))
            .Where(card => !card.EnergyCost.CostsX && card.EnergyCost.GetWithModifiers(CostModifiers.All) == 0)
            .ToList();

        foreach (CardModel card in bottomCards)
        {
            if(card.TargetType == TargetType.AnyEnemy)
            {
                Creature target = base.Owner.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
                await CardCmd.AutoPlay(choiceContext, card, target);
            }
            else if(card.TargetType == TargetType.Self)
            {
                await CardCmd.AutoPlay(choiceContext, card, Owner.Creature);
            }
            else
            {
                await CardCmd.AutoPlay(choiceContext, card, null);
            }
        }
    }

    private Creature? GetTarget(CardModel card)
    {
        if (card.TargetType == TargetType.Self)
            return Owner.Creature;

        if (card.TargetType == TargetType.AnyEnemy || card.TargetType == TargetType.AllEnemies)
        {
            var enemies = CombatState.GetOpponentsOf(Owner.Creature).Where(enemy => enemy.IsAlive).ToList();
            return enemies.Count == 0 ? null : Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        }

        return null;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2m);
    }
}
