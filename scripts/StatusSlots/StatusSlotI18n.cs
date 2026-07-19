using System.Collections.Generic;
using Godot;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 状态栏位本地化读取器。
/// 从 localization/{lang}/status_slot.json 读取文本，支持多语言自动切换。
/// </summary>
public static class StatusSlotI18n
{
    private const string FileStem = "status_slot";
    private static Dictionary<string, string> _dict = new();
    private static string _currentLang = "";

    /// <summary>
    /// 获取翻译文本。找不到返回 key 本身。
    /// </summary>
    public static string Get(string key)
    {
        EnsureLoaded();
        return _dict.TryGetValue(key, out var val) ? val : key;
    }

    /// <summary>
    /// 获取槽位名称。
    /// </summary>
    public static string GetSlotName(StatusSlotType type) => Get($"slot.{type.ToString().ToLowerInvariant()}.name");

    /// <summary>
    /// 获取槽位空状态提示。
    /// </summary>
    public static string GetSlotEmpty(StatusSlotType type) => Get($"slot.{type.ToString().ToLowerInvariant()}.empty");

    /// <summary>
    /// 获取效果名称。
    /// </summary>
    public static string GetEffectName(string effectKey) => Get($"effect.{effectKey}.name");

    /// <summary>
    /// 获取效果描述。
    /// </summary>
    public static string GetEffectDesc(string effectKey) => Get($"effect.{effectKey}.desc");

    private static void EnsureLoaded()
    {
        var lang = GetCurrentLang();
        if (lang == _currentLang && _dict.Count > 0) return;
        _currentLang = lang;
        _dict = LoadLang(lang);
    }

    private static string GetCurrentLang()
    {
        try
        {
            var locManager = MegaCrit.Sts2.Core.Localization.LocManager.Instance;
            if (locManager != null)
            {
                var lang = locManager.Language;
                if (!string.IsNullOrEmpty(lang)) return lang;
            }
        }
        catch { }

        // fallback
        return "zhs";
    }

    private static Dictionary<string, string> LoadLang(string lang)
    {
        var result = new Dictionary<string, string>();

        // 先尝试加载指定语言
        var path = $"res://Arknights_Mizuki/localization/{lang}/{FileStem}.json";
        if (!ResourceLoader.Exists(path))
        {
            // fallback 到 zhs
            path = $"res://Arknights_Mizuki/localization/zhs/{FileStem}.json";
        }

        if (!ResourceLoader.Exists(path))
        {
            Entry.Logger.Error($"[StatusSlot] i18n file not found: {path}");
            return result;
        }

        try
        {
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                Entry.Logger.Error($"[StatusSlot] Cannot open i18n file: {path}");
                return result;
            }

            var text = file.GetAsText();
            var json = Json.ParseString(text);
            if (json.VariantType == Variant.Type.Dictionary)
            {
                var dict = json.AsGodotDictionary();
                foreach (var (k, v) in dict)
                {
                    result[k.AsString()] = v.AsString();
                }
            }
            Entry.Logger.Info($"[StatusSlot] i18n loaded {result.Count} entries for lang={lang}");
        }
        catch (System.Exception ex)
        {
            Entry.Logger.Error($"[StatusSlot] i18n load failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 强制重新加载（语言切换后调用）。
    /// </summary>
    public static void Reload()
    {
        _currentLang = "";
        _dict.Clear();
    }
}
