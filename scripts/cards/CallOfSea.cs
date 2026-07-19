using STS2RitsuLib.Cards.DynamicVars;
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
public sealed class CallOfSea : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[4]
    {
        (DynamicVar)new DynamicVar("Float", 5m),
        (DynamicVar)new DynamicVar("Harvest", 5m),
        (DynamicVar)new CardsVar(2),
        (DynamicVar)new PowerVar<CallOfSeaPower>(1m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(Monster1.monster1),
        HoverTipFactory.FromKeyword(Monster1des.monster1des),
        HoverTipFactory.FromKeyword(Monster2.monster2),
        HoverTipFactory.FromKeyword(Monster2des.monster2des),
        HoverTipFactory.FromPower<CallOfSeaPower>()
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/CallOfSea.png";

    public CallOfSea() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AddOrEmpowerFloatingSea(choiceContext);
        await AddOrEmpowerHarvest(choiceContext);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await PowerCmd.Apply<CallOfSeaPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["CallOfSeaPower"].BaseValue,
            Owner.Creature,
            (CardModel)(object)this);
    }

    private async Task AddOrEmpowerFloatingSea(PlayerChoiceContext choiceContext)
    {
        if (!HasMinion<FloatingSeaMinion>(Owner))
        {
            _ = await MinionCmd.AddMinion<FloatingSeaMinion>(choiceContext,Owner, new MinionSummonOptions(
                MaxHp: 6m,
                PrimaryStatAmount: 2m,
                Source: this,
                Position: MinionPosition.Front));
            return;
        }

        Creature? pet = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster is FloatingSeaMinion);
        await PowerCmd.Apply<SeabornizationPower>(choiceContext, pet, DynamicVars["Float"].BaseValue, pet, this);
    }

    private async Task AddOrEmpowerHarvest(PlayerChoiceContext choiceContext)
    {
        if (!HasMinion<HarvestMinion>(Owner))
        {
            _ = await MinionCmd.AddMinion<HarvestMinion>(choiceContext,Owner, new MinionSummonOptions(
                MaxHp: 8m,
                PrimaryStatAmount: 2m,
                Source: this,
                Position: MinionPosition.Front));
            return;
        }

        Creature? pet = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster is HarvestMinion);
        await PowerCmd.Apply<SeabornizationPower>(choiceContext, pet, DynamicVars["Harvest"].BaseValue, pet, this);
    }

    private static bool HasMinion<T>(Player player) where T : MinionModel
    {
        return player.PlayerCombatState?.Pets.Any(p =>
            p is { IsAlive: true, IsPet: true, Monster: T }
        ) == true;
    }
    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars["Float"].UpgradeValueBy(2);
        DynamicVars["Harvest"].UpgradeValueBy(2);
    }
}
