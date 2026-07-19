using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Events;


[RegisterSharedEvent]
public sealed class XuanGaoZhiZang : ModEventTemplate
{
    private const string GoldKey = "Gold";

    public override string? CustomInitialPortraitPath => "res://Arknights_Mizuki/images/events/xuangaozhizang.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(GoldKey, 40m)
    };
    public override bool IsAllowed(IRunState runState)
    {
        return runState is RunState state && state.CurrentActIndex == 0 && !state.VisitedEventIds.Contains(Id);
    }

    public override void CalculateVars()
    {
        DynamicVars[GoldKey].BaseValue = Rng.NextInt(40, 61);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            new EventOption(this, Pray, OptionKey("PRAY"), HoverTipFactory.FromRelic<Key>()),
            new EventOption(this, Disrupt, OptionKey("DISRUPT"), HoverTipFactory.FromCardWithCardHoverTips<Regret>().Concat(HoverTipFactory.FromRelic<Key>()))
        };
    }

    private string OptionKey(string option)
    {
        return $"{Id.Entry}.pages.INITIAL.options.{option}";
    }

    private async Task Pray()
    {
        await GainKeyCharges(1);
        await PlayerCmd.GainGold(DynamicVars[GoldKey].BaseValue, Owner!);
        SetEventFinished(PageDescription("PRAY"));
    }

    private async Task Disrupt()
    {
        await CardPileCmd.AddCurseToDeck<Regret>(Owner!);
        await GainKeyCharges(2);
        SetEventFinished(PageDescription("DISRUPT"));
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
