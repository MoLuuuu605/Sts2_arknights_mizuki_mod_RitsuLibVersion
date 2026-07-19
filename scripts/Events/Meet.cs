using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Events;


[RegisterSharedEvent]
public sealed class Meet : ModEventTemplate
{
    private const int InspectHpLoss = 6;
    private const int RescueHpLoss = 12;

    public override string? CustomInitialPortraitPath => "res://Arknights_Mizuki/images/events/Meet.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new GoldVar(60)
    };
    public override bool IsAllowed(IRunState runState)
    {
        return runState is RunState state && state.CurrentActIndex == 0 && !state.VisitedEventIds.Contains(Id);
    }

    public override void CalculateVars()
    {
        DynamicVars.Gold.BaseValue = Rng.NextInt(60, 91);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            new EventOption(this, Inspect, OptionKey("INSPECT")).ThatDoesDamage(InspectHpLoss),
            new EventOption(this, Rescue, OptionKey("RESCUE")).ThatDoesDamage(RescueHpLoss),
            new EventOption(this, Leave, OptionKey("LEAVE"))
        };
    }

    private string OptionKey(string option)
    {
        return $"{Id.Entry}.pages.INITIAL.options.{option}";
    }

    private async Task Inspect()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, InspectHpLoss, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        await PlayerCmd.GainGold(DynamicVars.Gold.IntValue, Owner);
        SetEventFinished(PageDescription("INSPECT"));
    }

    private async Task Rescue()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, RescueHpLoss, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        await GainKeyCharges(1);
        SetEventFinished(PageDescription("RESCUE"));
    }

    private Task Leave()
    {
        SetEventFinished(PageDescription("LEAVE"));
        return Task.CompletedTask;
    }

    private async Task GainKeyCharges(int amount)
    {
        Key? key = Owner!.GetRelic<Key>();
        if (key == null)
        {
            key = await RelicCmd.Obtain<Key>(Owner);
            amount--;
        }

        key.AddCharges(amount);
    }
}
