using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.keywords;
using Arknights_Mizuki.Scripts.Minions;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.StatusSlots;
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
public class SeaBlessing : ModCardTemplate
{
    private const string EchoCountdownKey = "EchoCountdown";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new DynamicVar("Float", 2m),
        (DynamicVar)new DynamicVar(EchoCountdownKey, 2m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(Monster1.monster1),
        HoverTipFactory.FromKeyword(Monster1des.monster1des),
        HoverTipFactory.FromPower<SeabornizationPower>(),
        HoverTipFactory.FromKeyword(((CardModel)this).IsUpgraded ? Echo1.Echo : Echo2.Echo)
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/SeaBlessing.png";

    public SeaBlessing() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return;

        if (!StatusSlotManager.IsPlayerEligibleForSlot(Owner, StatusSlotType.SwarmCall))
            return;

        if (Pile.Type != PileType.Hand)
            return;

        DynamicVars[EchoCountdownKey].BaseValue -= 1;
        if (DynamicVars[EchoCountdownKey].IntValue > 0)
            return;

        DynamicVars["Float"].BaseValue += 1;
        ResetEchoCountdown();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        if (!HasMinion<FloatingSeaMinion>(Owner))
        {
            _ = await MinionCmd.AddMinion<FloatingSeaMinion>(choiceContext,Owner, new MinionSummonOptions(
                MaxHp: 6m,
                PrimaryStatAmount: 2m,
                Source: this,
                Position: MinionPosition.Front));
        }
        else
        {
            Creature? pet = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster is FloatingSeaMinion);
            await PowerCmd.Apply<SeabornizationPower>(
                choiceContext,
                pet,
                DynamicVars["Float"].BaseValue,
                pet,
                (CardModel)(object)this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[EchoCountdownKey].BaseValue = 1;
    }

    private void ResetEchoCountdown()
    {
        DynamicVars[EchoCountdownKey].BaseValue = ((CardModel)this).IsUpgraded ? 1 : 2;
    }

    private static bool HasMinion<T>(Player player) where T : MinionModel
    {
        return player.PlayerCombatState?.Pets.Any(p =>
            p is { IsAlive: true, IsPet: true, Monster: T }
        ) == true;
    }
}
