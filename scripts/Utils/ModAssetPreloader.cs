using System.Collections.Concurrent;
using Godot;

namespace Arknights_Mizuki.Scripts.Utils;

public static class ModAssetPreloader
{
    private static readonly ConcurrentDictionary<string, Resource> CachedAssets = new();

    private static readonly string[] CombatScenePaths =
    {
        "res://Arknights_Mizuki/monsters/floater.tscn",
        "res://Arknights_Mizuki/monsters/crawler.tscn",
        "res://Arknights_Mizuki/monsters/harvest.tscn",
    };

    private static readonly string[] CardImagePaths =
    {
        "res://Arknights_Mizuki/images/cards/Float.png",
        "res://Arknights_Mizuki/images/cards/Harvest.png",
    };

    public static void PreloadCombatAssets()
    {
        foreach (string path in CombatScenePaths)
            LoadAsset<PackedScene>(path);

        foreach (string path in CardImagePaths)
            LoadAsset<Texture2D>(path);
    }

    private static void LoadAsset<T>(string path) where T : Resource
    {
        if (CachedAssets.ContainsKey(path) || !ResourceLoader.Exists(path))
            return;

        T? asset = ResourceLoader.Load<T>(path, null, ResourceLoader.CacheMode.Reuse);
        if (asset != null)
            CachedAssets[path] = asset;
    }
}
