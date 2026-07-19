using STS2RitsuLib;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;
using Arknights_Mizuki.Scripts.Settings;

namespace Arknights_Mizuki.Scripts.StatusSlots;

internal sealed class MizukiModSettingsData
{
    public bool RevelationEnabled { get; set; } = StatusSlotSettings.DefaultRevelationEnabled;
    public bool AberrationEnabled { get; set; } = StatusSlotSettings.DefaultAberrationEnabled;
    public bool SwarmCallEnabled { get; set; } = StatusSlotSettings.DefaultSwarmCallEnabled;
    public bool RevelationAffectsOtherCharacters { get; set; } = StatusSlotSettings.DefaultRevelationAffectsOtherCharacters;
    public bool AberrationAffectsOtherCharacters { get; set; } = StatusSlotSettings.DefaultAberrationAffectsOtherCharacters;
    public bool SwarmCallAffectsOtherCharacters { get; set; } = StatusSlotSettings.DefaultSwarmCallAffectsOtherCharacters;
    public double SwarmCallChance { get; set; } = StatusSlotSettings.DefaultSwarmCallChance;
    public double RevelationChance { get; set; } = StatusSlotSettings.DefaultRevelationChance;
    public double AberrationHpThreshold { get; set; } = StatusSlotSettings.DefaultAberrationHpThreshold;
    public double AberrationChance { get; set; } = StatusSlotSettings.DefaultAberrationChance;
    public SublimationEventMode SublimationEventMode { get; set; } = FourthActSettings.DefaultSublimationEventMode;
    public bool SublimationEventAffectsOtherCharacters { get; set; } = FourthActSettings.DefaultSublimationEventAffectsOtherCharacters;
    public double NormalCombatScaling { get; set; } = FourthActSettings.DefaultNormalCombatScaling;
    public double BossCombatScaling { get; set; } = FourthActSettings.DefaultBossCombatScaling;
}

/// <summary>
/// 基于 RitsuLib ModSettings 的三个开关配置。
/// 分别控制启示、排异反应、回响是否启用。
/// </summary>
internal static class StatusSlotSettings
{
    internal const string SettingsDataKey = "mod_settings";
    public const bool DefaultRevelationEnabled = true;
    public const bool DefaultAberrationEnabled = true;
    public const bool DefaultSwarmCallEnabled = true;
    public const bool DefaultRevelationAffectsOtherCharacters = false;
    public const bool DefaultAberrationAffectsOtherCharacters = false;
    public const bool DefaultSwarmCallAffectsOtherCharacters = false;
    public const double DefaultSwarmCallChance = 0.8;
    public const double DefaultRevelationChance = 0.5;
    public const double DefaultAberrationHpThreshold = 10.0;
    public const double DefaultAberrationChance = 0.5;

    public static IModSettingsValueBinding<bool> RevelationEnabled { get; private set; } = null!;
    public static IModSettingsValueBinding<bool> AberrationEnabled { get; private set; } = null!;
    public static IModSettingsValueBinding<bool> SwarmCallEnabled { get; private set; } = null!;
    public static IModSettingsValueBinding<bool> RevelationAffectsOtherCharacters { get; private set; } = null!;
    public static IModSettingsValueBinding<bool> AberrationAffectsOtherCharacters { get; private set; } = null!;
    public static IModSettingsValueBinding<bool> SwarmCallAffectsOtherCharacters { get; private set; } = null!;
    public static IModSettingsValueBinding<double> SwarmCallChance { get; private set; } = null!;
    public static IModSettingsValueBinding<double> RevelationChance { get; private set; } = null!;
    public static IModSettingsValueBinding<double> AberrationHpThreshold { get; private set; } = null!;
    public static IModSettingsValueBinding<double> AberrationChance { get; private set; } = null!;

    private const string ModId = Entry.ModId;

    public static void Register()
    {
        using (RitsuLibFramework.BeginModDataRegistration(ModId))
        {
            RitsuLibFramework.GetDataStore(ModId).Register(
                SettingsDataKey,
                "settings",
                SaveScope.Global,
                () => new MizukiModSettingsData(),
                autoCreateIfMissing: true);
        }

        FourthActSettings.RegisterBindings();

        RevelationEnabled = Global(data => data.RevelationEnabled, (data, value) => data.RevelationEnabled = value, DefaultRevelationEnabled);
        AberrationEnabled = Global(data => data.AberrationEnabled, (data, value) => data.AberrationEnabled = value, DefaultAberrationEnabled);
        SwarmCallEnabled = Global(data => data.SwarmCallEnabled, (data, value) => data.SwarmCallEnabled = value, DefaultSwarmCallEnabled);
        RevelationAffectsOtherCharacters = Global(data => data.RevelationAffectsOtherCharacters, (data, value) => data.RevelationAffectsOtherCharacters = value, DefaultRevelationAffectsOtherCharacters);
        AberrationAffectsOtherCharacters = Global(data => data.AberrationAffectsOtherCharacters, (data, value) => data.AberrationAffectsOtherCharacters = value, DefaultAberrationAffectsOtherCharacters);
        SwarmCallAffectsOtherCharacters = Global(data => data.SwarmCallAffectsOtherCharacters, (data, value) => data.SwarmCallAffectsOtherCharacters = value, DefaultSwarmCallAffectsOtherCharacters);
        SwarmCallChance = Global(data => data.SwarmCallChance, (data, value) => data.SwarmCallChance = value, DefaultSwarmCallChance);
        RevelationChance = Global(data => data.RevelationChance, (data, value) => data.RevelationChance = value, DefaultRevelationChance);
        AberrationHpThreshold = Global(data => data.AberrationHpThreshold, (data, value) => data.AberrationHpThreshold = value, DefaultAberrationHpThreshold);
        AberrationChance = Global(data => data.AberrationChance, (data, value) => data.AberrationChance = value, DefaultAberrationChance);

        RitsuLibFramework.RegisterModSettings(ModId, page =>
        {
            page.WithTitle(ModSettingsText.Literal("Arknights Mizuki 设置"));
            page.WithDescription(ModSettingsText.Literal(
                "控制状态栏位与第四层入口事件。联机时，启示、排异反应和回响统一使用主机配置。"));

            page.AddSection("revelation", section =>
            {
                section.AddToggle(
                    "revelation_toggle",
                    ModSettingsText.Literal("启用启示系统"),
                    RevelationEnabled,
                    ModSettingsText.Literal("开启后，游戏中可获得的启示效果将在顶栏显示并生效。"));
                section.AddToggle(
                    "revelation_other_characters_toggle",
                    ModSettingsText.Literal("允许非水月角色使用启示"),
                    RevelationAffectsOtherCharacters,
                    ModSettingsText.Literal("默认关闭。关闭时，非水月跑局不会获得或触发启示机制。"));
                section.AddSlider(
                    "revelation_chance",
                    ModSettingsText.Literal("启示获得概率"),
                    RevelationChance,
                    0.0, 1.0, 0.05,
                    v => $"{v * 100:F0}%",
                    ModSettingsText.Literal("每层开始时获得启示的概率。默认 50%。"));
            });

            page.AddSection("aberration", section =>
            {
                section.AddToggle(
                    "aberration_toggle",
                    ModSettingsText.Literal("启用排异反应系统"),
                    AberrationEnabled,
                    ModSettingsText.Literal("开启后，游戏中可获得的排异反应效果将在顶栏显示并生效。"));
                section.AddToggle(
                    "aberration_other_characters_toggle",
                    ModSettingsText.Literal("允许非水月角色使用排异反应"),
                    AberrationAffectsOtherCharacters,
                    ModSettingsText.Literal("默认关闭。关闭时，非水月跑局不会获得或触发排异反应机制。"));
                section.AddSlider(
                    "aberration_hp_threshold",
                    ModSettingsText.Literal("受伤阈值"),
                    AberrationHpThreshold,
                    5, 50, 1,
                    v => $"{v:F0}",
                    ModSettingsText.Literal("单场战斗失去生命达到此值时触发判定。默认 10。"));
                section.AddSlider(
                    "aberration_chance",
                    ModSettingsText.Literal("排异获得概率"),
                    AberrationChance,
                    0.0, 1.0, 0.05,
                    v => $"{v * 100:F0}%",
                    ModSettingsText.Literal("达到受伤阈值后获得排异反应的概率。默认 50%。"));
            });

            page.AddSection("swarm_call", section =>
            {
                section.AddToggle(
                    "swarm_call_toggle",
                    ModSettingsText.Literal("启用回响系统"),
                    SwarmCallEnabled,
                    ModSettingsText.Literal("开启后，游戏中可获得的回响效果将在顶栏显示并生效。"));
                section.AddToggle(
                    "swarm_call_other_characters_toggle",
                    ModSettingsText.Literal("允许非水月角色使用回响"),
                    SwarmCallAffectsOtherCharacters,
                    ModSettingsText.Literal("默认关闭。关闭时，非水月跑局不会获得或触发回响机制。"));
                section.AddSlider(
                    "swarm_call_chance",
                    ModSettingsText.Literal("回响获得概率"),
                    SwarmCallChance,
                    0.0, 1.0, 0.05,
                    v => $"{v * 100:F0}%",
                    ModSettingsText.Literal("每层开始时获得回响的概率。默认 80%。"));
            });

            page.AddSection("fourth_act", section =>
            {
                section.AddEnumChoice(
                    "sublimation_event_mode",
                    ModSettingsText.Literal("升华事件出现方式"),
                    FourthActSettings.SublimationMode,
                    mode => ModSettingsText.Literal(mode switch
                    {
                        SublimationEventMode.Disabled => "关闭",
                        SublimationEventMode.EventPool => "仅加入事件池",
                        _ => "强制第一个问号房"
                    }),
                    ModSettingsText.Literal("控制升华事件是否出现，以及是否替换第三层遇到的第一个问号房。"));
                section.AddToggle(
                    "sublimation_event_other_characters_toggle",
                    ModSettingsText.Literal("允许其他角色遇到升华事件"),
                    FourthActSettings.SublimationEventAffectsOtherCharacters,
                    ModSettingsText.Literal("默认关闭。关闭时，只有水月角色能触发升华事件。"));
                section.WithEntryEnabledWhen(
                    "sublimation_event_other_characters_toggle",
                    () => FourthActSettings.CurrentSublimationMode != SublimationEventMode.Disabled);
                section.AddSlider(
                    "fourth_act_normal_combat_scaling",
                    ModSettingsText.Literal("第四层普通战联机倍率"),
                    FourthActSettings.NormalCombatScaling,
                    1.0, 3.0, 0.05,
                    value => $"{value:F2}x",
                    ModSettingsText.Literal("第四层非 Boss 战斗的联机难度倍率。默认 1.30x，由主机配置。"));
                section.AddSlider(
                    "fourth_act_boss_combat_scaling",
                    ModSettingsText.Literal("第四层 Boss 联机倍率"),
                    FourthActSettings.BossCombatScaling,
                    1.0, 3.0, 0.05,
                    value => $"{value:F2}x",
                    ModSettingsText.Literal("第四层 Boss 战斗的联机难度倍率。默认 1.40x，由主机配置。"));
            });
        });
    }

    public static bool IsRevelationEnabled => RevelationEnabled.Read();
    public static bool IsAberrationEnabled => AberrationEnabled.Read();
    public static bool IsSwarmCallEnabled => SwarmCallEnabled.Read();
    public static bool DoesRevelationAffectOtherCharacters => RevelationAffectsOtherCharacters.Read();
    public static bool DoesAberrationAffectOtherCharacters => AberrationAffectsOtherCharacters.Read();
    public static bool DoesSwarmCallAffectOtherCharacters => SwarmCallAffectsOtherCharacters.Read();
    public static double SwarmCallChanceValue => SwarmCallChance.Read();
    public static double RevelationChanceValue => RevelationChance.Read();
    public static int AberrationHpThresholdValue => (int)AberrationHpThreshold.Read();
    public static double AberrationChanceValue => AberrationChance.Read();

    internal static IModSettingsValueBinding<TValue> Global<TValue>(
        Func<MizukiModSettingsData, TValue> getter,
        Action<MizukiModSettingsData, TValue> setter,
        TValue defaultValue)
    {
        return ModSettingsBindings.WithDefault(
            ModSettingsBindings.Global(ModId, SettingsDataKey, getter, setter),
            () => defaultValue);
    }
}
