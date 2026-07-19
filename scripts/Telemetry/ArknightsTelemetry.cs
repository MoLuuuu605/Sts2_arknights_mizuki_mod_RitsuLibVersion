using System.Reflection;
using STS2RitsuLib;
using STS2RitsuLib.Telemetry;

namespace Arknights_Mizuki.Scripts.Telemetry;

public static class ArknightsTelemetry
{
    private const string ApplicantId = "arknights_mizuki_telemetry";

    private const string TelemetryEndpoint = "http://mc.rfoc.top:30202/telemetry";

    public static void Register(string modId)
    {
        RitsuLibFramework.RegisterTelemetryApplicant(new TelemetryApplicant
        {
            ApplicantId = ApplicantId,
            OwnerModId = modId,
            DisplayName = "Arknights Mizuki",
            Adapter = CreateAdapter(),
            Requests =
            [
                TelemetryRequest.BasicUsage("收集匿名启动和基础使用情况，用于判断 mod 是否正常加载。"),

                TelemetryRequest.RunHistory("收集水月角色的匿名通关/失败运行记录，用于平衡性分析。", [], _ => true),

                TelemetryRequest.ModInventory("收集启用的 mod 列表，用于排查兼容性问题。")
            ]
        });

        RitsuLibFramework.SubscribeLifecycle<TelemetryStartupSnapshotReadyEvent>(_ => CaptureModLoaded(), true);
    }

    private static ITelemetryAdapter CreateAdapter()
    {
        if (string.IsNullOrWhiteSpace(TelemetryEndpoint))
            return new DisabledTelemetryAdapter("arknights_mizuki_telemetry_disabled_until_endpoint_is_configured");

        return new HttpJsonTelemetryAdapter(TelemetryEndpoint, new Dictionary<string, string>());
    }

    private static void CaptureModLoaded()
    {
        ITelemetryClient client = TelemetryApi.GetClient(ApplicantId);
        if (!client.IsEnabled("basic_usage"))
            return;

        Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        client.Capture(
            "mod_loaded",
            "basic_usage",
            new Dictionary<string, object?>
            {
                ["mod_id"] = "Arknights_Mizuki",
                ["assembly_version"] = version?.ToString() ?? "unknown"
            });
    }
}
