using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Events;


[RegisterSharedEvent]
public sealed class MeetInDream : ModEventTemplate
{
    private const int VoiceCost = 55;
    private const int PatternCost = 85;
    private const int InfoHpLoss = 9;
    private const int JoinHpLoss = 18;
    private const int CloserHpLoss = 9;

    public override string? CustomInitialPortraitPath => "res://Arknights_Mizuki/images/events/Meetindream.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new MaxHpVar(3),
        new GoldVar(30)
    };
    public override bool IsAllowed(IRunState runState)
    {
        return runState is RunState state && state.CurrentActIndex == 1 && !state.VisitedEventIds.Contains(Id);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return GenerateDreamOptions();
    }

    private IReadOnlyList<EventOption> GenerateDreamOptions()
    {
        List<EventOption> options = new()
        {
            CreateVoiceOption(),
            CreatePatternOption(),
            new EventOption(this, SummarizeInfo, OptionKey("SUMMARIZE_INFO")).ThatDoesDamage(InfoHpLoss),
            new EventOption(this, JoinConversation, OptionKey("JOIN_CONVERSATION")).ThatDoesDamage(JoinHpLoss),
            new EventOption(this, MoveCloser, OptionKey("MOVE_CLOSER")).ThatDoesDamage(CloserHpLoss),
            CreateRecordOption()
        };

        List<EventOption> selected = options
            .UnstableShuffle(Rng)
            .Take(3)
            .ToList();
        selected.Add(new EventOption(this, WakeUp, OptionKey("WAKE_UP")));
        return selected;
    }

    private EventOption CreateVoiceOption()
    {
        return new EventOption(this, Owner!.Gold >= VoiceCost ? DiscernVoice : null, OptionKey("DISCERN_VOICE"));
    }

    private EventOption CreatePatternOption()
    {
        return new EventOption(this, Owner!.Gold >= PatternCost ? SummarizePattern : null, OptionKey("SUMMARIZE_PATTERN"));
    }

    private EventOption CreateRecordOption()
    {
        return new EventOption(this, HasKey(2) ? RecordInfo : null, OptionKey("RECORD_INFO"));
    }

    private string OptionKey(string option)
    {
        return $"{Id.Entry}.pages.INITIAL.options.{option}";
    }

    private async Task DiscernVoice()
    {
        await PlayerCmd.LoseGold(VoiceCost, Owner!, GoldLossType.Spent);
        await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.BaseValue);
        ContinueDream("DISCERN_VOICE");
    }

    private async Task SummarizePattern()
    {
        await PlayerCmd.LoseGold(PatternCost, Owner!, GoldLossType.Spent);
        await GainKeyCharges(1);
        ContinueDream("SUMMARIZE_PATTERN");
    }

    private async Task SummarizeInfo()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, InfoHpLoss, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        await PlayerCmd.GainGold(Rng.NextInt(30, 60), Owner);
        ContinueDream("SUMMARIZE_INFO");
    }

    private async Task JoinConversation()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, JoinHpLoss, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        await ObtainRandomRelic();
        ContinueDream("JOIN_CONVERSATION");
    }

    private async Task MoveCloser()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, CloserHpLoss, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        await GainKeyCharges(1);
        ContinueDream("MOVE_CLOSER");
    }

    private async Task RecordInfo()
    {
        Key? key = Owner!.GetRelic<Key>();
        if (key == null || key.ChargesRemaining <= 0)
        {
            ContinueDream("RECORD_INFO");
            return;
        }

        key.ChargesRemaining--;
        key.ChargesRemaining--;
        await ObtainRandomRelic();
        ContinueDream("RECORD_INFO");
    }

    private Task WakeUp()
    {
        SetEventFinished(PageDescription("WAKE_UP"));
        return Task.CompletedTask;
    }

    private void ContinueDream(string page)
    {
        SetEventState(PageDescription(page), GenerateDreamOptions());
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

    private async Task ObtainRandomRelic()
    {
        RelicModel relic = RelicFactory.PullNextRelicFromFront(Owner!).ToMutable();
        await RelicCmd.Obtain(relic, Owner);
    }

    private bool HasKey(int amount = 1)
    {
        return Owner?.GetRelic<Key>()?.ChargesRemaining >= amount;
    }
}
