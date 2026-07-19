using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Characters;
using Arknights_Mizuki.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Arknights_Mizuki.Scripts.Events;


[RegisterSharedEvent]
public sealed class Sublimation : ModEventTemplate
{
    private const int DeterminationKeyCost = 3;
    private const int ObservationKeyCost = 1;

    public override string? CustomInitialPortraitPath => "res://Arknights_Mizuki/images/events/shenghua.png";

    public override bool IsShared => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("MaxHpGain", 20m)
    };
    public override bool IsAllowed(IRunState runState)
    {
        return runState is RunState state && state.CurrentActIndex == 2 && !state.VisitedEventIds.Contains(Id);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = null;
        if(!AllPlayersAreNotMizuki())
        {
            options = new()
            {
                new EventOption(this, AllPlayersHaveKeyCharges(DeterminationKeyCost) ? ChooseDetermination : null, OptionKey("CHOOSE_DETERMINATION")).WithRelic<Determination>(Owner),
                new EventOption(this, AllPlayersHaveKeyCharges(ObservationKeyCost) ? ChooseObservation : null, OptionKey("CHOOSE_OBSERVATION")).WithRelic<Observation>(Owner),
                new EventOption(this, ChooseHerd, OptionKey("CHOOSE_HERD")),
            };
        }
        else
        {
            options = new()
            {
                new EventOption(this,AllPlayersAreNotMizuki() ? ChooseDetermination : null, OptionKey("CHOOSE_DETERMINATION_OTHER")).WithRelic<Determination>(Owner),
                new EventOption(this, ChooseHerd, OptionKey("CHOOSE_HERD")),
            };
        }

        return options;
    }

    private string OptionKey(string option)
    {
        return $"{Id.Entry}.pages.INITIAL.options.{option}";
    }

    private bool AllPlayersHaveKeyCharges(int amount)
    {
        return Owner?.RunState.Players.All(player => HasKeyCharges(player, amount)) == true;
    }

    private static bool HasKeyCharges(MegaCrit.Sts2.Core.Entities.Players.Player player, int amount)
    {
        return player.GetRelic<Key>()?.ChargesRemaining >= amount;
    }

    private bool AllPlayersAreNotMizuki()
    {
        return Owner?.RunState.Players.All(player => player.Character is not Mizuki) == true;
    }

    private async Task ChooseDetermination()
    {
        SpendKeyCharges(DeterminationKeyCost);
        await RelicCmd.Obtain<Determination>(Owner!);
        SetEventFinished(PageDescription("DETERMINATION"));
    }

    private async Task ChooseDeterminationForOtherCharacter()
    {
        await RelicCmd.Obtain<Determination>(Owner!);
        SetEventFinished(PageDescription("DETERMINATION"));
    }

    private async Task ChooseObservation()
    {
        SpendKeyCharges(ObservationKeyCost);
        await RelicCmd.Obtain<Observation>(Owner!);
        SetEventFinished(PageDescription("OBSERVATION"));
    }

    private void SpendKeyCharges(int amount)
    {
        Key? key = Owner?.GetRelic<Key>();
        if (key == null)
        {
            return;
        }

        key.ChargesRemaining -= amount;
    }

    private async Task ChooseHerd()
    {
        await CardPileCmd.AddCurseToDeck<Doubt>(Owner!);
        await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars["MaxHpGain"].BaseValue);
        SetEventFinished(PageDescription("HERD"));
    }
}
