using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using Arknights_Mizuki.Scripts.Acts;
using Arknights_Mizuki.Scripts.Relics;
using Arknights_Mizuki.Scripts.StatusSlots;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Arknights_Mizuki.Scripts.keywords;
using MegaCrit.Sts2.Core.Random;

namespace Arknights_Mizuki.Scripts.Ancients;

[RegisterActAncient(typeof(EvolutionSingularityAct))]
public sealed class LastTidewatcher : ModAncientEventTemplate
{
    private const string Key = "ARKNIGHTS_MIZUKI_EVENT_LAST_TIDEWATCHER";

    public override Color ButtonColor => new Color(0.05f, 0.09f, 0.13f, 0.8f);
    public override Color DialogueColor => new Color("8ec8de");

    public override EventAssetProfile AssetProfile => new(
        BackgroundScenePath: "res://Arknights_Mizuki/scenes/events/last_tidewatcher.tscn"
    );

    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        MapIconPath: "res://Arknights_Mizuki/images/map/ancients/last_tidewatcher.png",
        MapIconOutlinePath: "res://Arknights_Mizuki/images/map/ancients/last_tidewatcher_outline.png",
        RunHistoryIconPath: "res://Arknights_Mizuki/images/ui/run_history/last_tidewatcher.png",
        RunHistoryIconOutlinePath: "res://Arknights_Mizuki/images/ui/run_history/last_tidewatcher_outline.png"
    );

    public override bool IsValidForAct(ActModel act) => act is EvolutionSingularityAct;

    // ── 悬浮框辅助 ─────────────────────────────────

    /// <summary>获取遗物的悬浮框信息</summary>
    private static IHoverTip[] RelicTips<T>() where T : RelicModel
        => HoverTipFactory.FromRelic(ModelDb.Relic<T>()).ToArray();
    // 池1：获得海洋的脉搏
   
    private IReadOnlyList<EventOption> Pool1 => new[]
    {
        new EventOption(this, () => ObtainOceanPulse(),
            $"{Key}.pages.INITIAL.options.OCEAN_PULSE", RelicTips<OceanPulse>()),
    };

    // 池2：失去所有金币获得深蓝之心 / 获得绝唱 / 获得提灯
    private IReadOnlyList<EventOption> Pool2 => new[]
    {
        new EventOption(this, () => LoseGoldForHeart(),
            $"{Key}.pages.INITIAL.options.HEART", RelicTips<DeepBlueHeart>()),
        new EventOption(this, () => ObtainSwanSong(),
            $"{Key}.pages.INITIAL.options.SWAN_SONG", RelicTips<SwanSong>()),
        new EventOption(this, () => ObtainLantern(),
            $"{Key}.pages.INITIAL.options.LANTERN", RelicTips<TidewatcherLantern>()),
    };

    // 池3：3个随机遗物 / 5个随机遗物 / 清除排异与回响获得众我
    private IReadOnlyList<EventOption> Pool3
    {
        get
        {
            var options = new List<EventOption>
            {
                new EventOption(this, () => ObtainRandomRelics(3),
                    $"{Key}.pages.INITIAL.options.THREE_RELICS"),
                new EventOption(this, () => ObtainRandomRelics(5),
                    $"{Key}.pages.INITIAL.options.FIVE_RELICS"),
            };

            if (CanOwnerUseSwarmCall)
            {
                options.Add(new EventOption(this, () => ClearAndGetAllMe(),
                    $"{Key}.pages.INITIAL.options.ALL_ME", HoverTipFactory.FromKeyword(ALLME.Allme)));
            }

            return options;
        }
    }

    private bool CanOwnerUseSwarmCall
        => Owner != null && StatusSlotManager.GetStatusSlotOwnerPlayer(StatusSlotType.SwarmCall) == Owner;

    public override IEnumerable<EventOption> AllPossibleOptions =>
        Pool1.Concat(Pool2).Concat(Pool3);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            Rng.NextItem(Pool1)!,
            Rng.NextItem(Pool2)!,
            Rng.NextItem(Pool3)!,
        };
    }

    protected override AncientDialogueSet DefineDialogues()
    {
        return new AncientDialogueSet
        {
            FirstVisitEverDialogue = new AncientDialogue(""),
            CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>(),
            AgnosticDialogues = new AncientDialogue[] { new AncientDialogue("") }
        };
    }

    // ── 选项实现 ──────────────────────────────────

    /// <summary>获得海洋的脉搏</summary>
    private async Task ObtainOceanPulse()
    {
        if (Owner == null) { Done(); return; }
        await RelicCmd.Obtain<OceanPulse>(Owner);
        Done();
    }

    /// <summary>失去所有金币，获得深蓝之心</summary>
    private async Task LoseGoldForHeart()
    {
        if (Owner == null) { Done(); return; }
        await PlayerCmd.LoseGold(Owner.Gold, Owner, GoldLossType.Spent);
        await RelicCmd.Obtain<DeepBlueHeart>(Owner);
        Done();
    }

    /// <summary>获得绝唱</summary>
    private async Task ObtainSwanSong()
    {
        if (Owner == null) { Done(); return; }
        await RelicCmd.Obtain<SwanSong>(Owner);
        Done();
    }

    /// <summary>获得提灯</summary>
    private async Task ObtainLantern()
    {
        if (Owner == null) { Done(); return; }
        await RelicCmd.Obtain<TidewatcherLantern>(Owner);
        Done();
    }

    /// <summary>获得 N 个随机遗物</summary>
    private async Task ObtainRandomRelics(int count)
    {
        if (Owner == null) { Done(); return; }
        if (count == 5)
        {
            var MaxHp=Owner.Creature.MaxHp;
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(),Owner.Creature,(decimal)(MaxHp*0.2),false);
        }
        for (int i = 0; i < count; i++)
        {
            RelicModel relic = RelicFactory.PullNextRelicFromFront(Owner!).ToMutable();
            await RelicCmd.Obtain(relic, Owner);
        }
        Done();
    }

    /// <summary>清除现有的排异反应与回响，获得回响：众我</summary>
    private async Task ClearAndGetAllMe()
    {
        if (Owner == null) { Done(); return; }
        if (!CanOwnerUseSwarmCall) { Done(); return; }

        await StatusSlotManager.RemoveEffectAsync(StatusSlotType.Aberration);
        await StatusSlotManager.RemoveEffectAsync(StatusSlotType.SwarmCall);
        await StatusSlotManager.AssignEffectAsync(StatusSlotType.SwarmCall, "echo_allme");
        Done();
    }
}
