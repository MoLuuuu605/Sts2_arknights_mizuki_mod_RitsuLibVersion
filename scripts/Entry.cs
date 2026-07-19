using System.Reflection;
using Arknights_Mizuki.Scripts.StatusSlots;
using Arknights_Mizuki.Scripts.Telemetry;
using Arknights_Mizuki.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;

namespace Arknights_Mizuki.Scripts;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public const string ModId = "Arknights_Mizuki";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Main()
    {
        Init();
    }

    public static void Init()
    {
        var harmony = new Harmony("arknights_mizuki");
        harmony.PatchAll(typeof(Entry).Assembly);

        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        StatusSlotRunData.Register();
        StatusSlotSettings.Register();
        ArknightsTelemetry.Register(ModId);
        ModAssetPreloader.PreloadCombatAssets();

        // 注册状态栏位事件观察者（singleton，接收游戏生命周期事件）
        RitsuLibFramework.SubscribeLifecycle(StatusSlotObserver.Instance);

        // 安装状态栏顶栏 UI
        StatusSlotPatches.Install(harmony);

        Logger.Info("[StatusSlot] Mod initialized, observer subscribed");
    }
}
