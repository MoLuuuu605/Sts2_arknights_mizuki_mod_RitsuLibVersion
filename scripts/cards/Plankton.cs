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
public sealed class Plankton : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
    {
        (DynamicVar)new HealVar(8m),
        (DynamicVar)new DynamicVar("Float", 3m),
        (DynamicVar)new DynamicVar("Times", 2m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(Monster1.monster1),
        HoverTipFactory.FromKeyword(Monster1des.monster1des),
        HoverTipFactory.FromPower<SeabornizationPower>()
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/Plankton.png";

    public Plankton() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);

        for (int i = 0; i < DynamicVars["Times"].IntValue; i++)
        {
            await Float(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Float"].UpgradeValueBy(2m);
    }

    private async Task Float(PlayerChoiceContext choiceContext)
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
        await PowerCmd.Apply<SeabornizationPower>(
            choiceContext,
            pet,
            DynamicVars["Float"].BaseValue,
            pet,
            (CardModel)(object)this);
    }

    private static bool HasMinion<T>(Player player) where T : MinionModel
    {
        return player.PlayerCombatState?.Pets.Any(p =>
            p is { IsAlive: true, IsPet: true, Monster: T }
        ) == true;
    }
}
