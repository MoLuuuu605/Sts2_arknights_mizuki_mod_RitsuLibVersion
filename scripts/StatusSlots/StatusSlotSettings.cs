using STS2RitsuLib;
using STS2RitsuLib.Settings;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 基于 RitsuLib ModSettings 的三个开关配置。
/// 分别控制启示、排异反应、回响是否启用。
/// </summary>
internal static class StatusSlotSettings
{
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
        // 默认全部启用
        RevelationEnabled = ModSettingsBindings.InMemory<bool>(ModId, "status_slot.revelation_enabled", DefaultRevelationEnabled);
        AberrationEnabled = ModSettingsBindings.InMemory<bool>(ModId, "status_slot.aberration_enabled", DefaultAberrationEnabled);
        SwarmCallEnabled = ModSettingsBindings.InMemory<bool>(ModId, "status_slot.swarm_call_enabled", DefaultSwarmCallEnabled);
        RevelationAffectsOtherCharacters = ModSettingsBindings.InMemory<bool>(ModId, "status_slot.revelation_affects_other_characters", DefaultRevelationAffectsOtherCharacters);
        AberrationAffectsOtherCharacters = ModSettingsBindings.InMemory<bool>(ModId, "status_slot.aberration_affects_other_characters", DefaultAberrationAffectsOtherCharacters);
        SwarmCallAffectsOtherCharacters = ModSettingsBindings.InMemory<bool>(ModId, "status_slot.swarm_call_affects_other_characters", DefaultSwarmCallAffectsOtherCharacters);
        SwarmCallChance = ModSettingsBindings.InMemory<double>(ModId, "status_slot.swarm_call_chance", DefaultSwarmCallChance);
        RevelationChance = ModSettingsBindings.InMemory<double>(ModId, "status_slot.revelation_chance", DefaultRevelationChance);
        AberrationHpThreshold = ModSettingsBindings.InMemory<double>(ModId, "status_slot.aberration_hp_threshold", DefaultAberrationHpThreshold);
        AberrationChance = ModSettingsBindings.InMemory<double>(ModId, "status_slot.aberration_chance", DefaultAberrationChance);

        RitsuLibFramework.RegisterModSettings(ModId, page =>
        {
            page.WithTitle(ModSettingsText.Literal("Arknights Mizuki 状态栏位"));
            page.WithDescription(ModSettingsText.Literal("控制启示、排异反应、回响系统的开关。"));

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
                    ModSettingsText.Literal("单场战斗失去生命超过此值时触发判定。默认 15。"));
                section.AddSlider(
                    "aberration_chance",
                    ModSettingsText.Literal("排异获得概率"),
                    AberrationChance,
                    0.0, 1.0, 0.05,
                    v => $"{v * 100:F0}%",
                    ModSettingsText.Literal("达到受伤阈值后获得排异反应的概率。默认 40%。"));
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
}
