using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.keywords;
using Arknights_Mizuki.Scripts.Minions;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Commands;
using MinionLib.Minion;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class GroupHatching : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
        (DynamicVar)new CardsVar(8),
        (DynamicVar)new DynamicVar("Assimilation", 1m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromCard<BabyHs>(),
        HoverTipFactory.FromPower<SeabornizationPower>(),
        HoverTipFactory.FromKeyword(Monster1.monster1),
        HoverTipFactory.FromKeyword(Monster1des.monster1des),
        HoverTipFactory.FromKeyword(Monster2.monster2),
        HoverTipFactory.FromKeyword(Monster2des.monster2des)

    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1] { CardKeyword.Exhaust };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/GroupHatching.png";

    public GroupHatching() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardPile drawPile = PileType.Draw.GetPile(Owner);
        List<CardModel> bottomCards = drawPile.Cards
            .Skip(Math.Max(0, drawPile.Cards.Count - DynamicVars.Cards.IntValue))
            .ToList();

        int consumedBabies = 0;
        foreach (CardModel card in bottomCards)
        {
            if (card is BabyHs)
                consumedBabies++;

            await CardCmd.Exhaust(choiceContext, card);
        }

        for (int i = 0; i < consumedBabies; i++)
        {
            if (Owner.RunState.Rng.Niche.NextItem(new[] { 0, 1 }) == 0)
                await Float(choiceContext, (CardModel)(object)this, Owner, DynamicVars["Assimilation"].BaseValue);
            else
                await Harvest(choiceContext, (CardModel)(object)this, Owner, DynamicVars["Assimilation"].BaseValue);
        }
    }

    public static async Task Float(PlayerChoiceContext choiceContext, CardModel source, Player owner, decimal amount)
    {
        if (!HasMinion<FloatingSeaMinion>(owner))
        {
            _ = await MinionCmd.AddMinion<FloatingSeaMinion>(choiceContext,owner, new MinionSummonOptions(MaxHp: 6m, PrimaryStatAmount: 2m, Source: source, Position: MinionPosition.Front));
            return;
        }

        Creature? pet = owner.PlayerCombatState?.Pets.FirstOrDefault(p => p is { IsAlive: true, IsPet: true, Monster: FloatingSeaMinion });
        await PowerCmd.Apply<SeabornizationPower>(choiceContext, pet, amount, pet, source);
    }

    public static async Task Harvest(PlayerChoiceContext choiceContext, CardModel source, Player owner, decimal amount)
    {
        if (!HasMinion<HarvestMinion>(owner))
        {
            _ = await MinionCmd.AddMinion<HarvestMinion>(choiceContext,owner, new MinionSummonOptions(MaxHp: 8m, PrimaryStatAmount: 2m, Source: source, Position: MinionPosition.Front));
            return;
        }

        Creature? pet = owner.PlayerCombatState?.Pets.FirstOrDefault(p => p is { IsAlive: true, IsPet: true, Monster: HarvestMinion });
        await PowerCmd.Apply<SeabornizationPower>(choiceContext, pet, amount, pet, source);
    }

    private static bool HasMinion<T>(Player player) where T : MinionModel
    {
        return player.PlayerCombatState?.Pets.Any(p => p is { IsAlive: true, IsPet: true, Monster: T }) == true;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Assimilation"].UpgradeValueBy(1m);
    }
}
