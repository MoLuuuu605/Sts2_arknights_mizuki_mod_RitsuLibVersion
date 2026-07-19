using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.sts2.Core.Nodes.TopBar;
using MegaCrit.Sts2.Core.Runs;
using System.Reflection;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// Harmony patch: NGlobalUi.Initialize 时安装顶栏框位，并隐藏数据 Modifier 图标。
/// </summary>
public static class StatusSlotPatches
{
    private static readonly FieldInfo? ModifiersContainerField =
        typeof(NTopBar).GetField("_modifiersContainer", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? TopBarModifierModelField =
        typeof(NTopBarModifier).GetField("_modifier", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void Install(Harmony harmony)
    {
        const BindingFlags AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        var method = typeof(NGlobalUi).GetMethod("Initialize", AllFlags, null, new[] { typeof(RunState) }, null);
        if (method == null)
        {
            Entry.Logger.Info("[StatusSlot] NGlobalUi.Initialize not found");
            return;
        }

        harmony.Patch(method, postfix: new HarmonyMethod(typeof(StatusSlotPatches), nameof(NGlobalUiInitializePostfix)));
        Entry.Logger.Info("[StatusSlot] Patched NGlobalUi.Initialize");
    }

    private static void NGlobalUiInitializePostfix(NGlobalUi __instance, RunState runState)
    {
        try
        {
            StatusSlotFrame.EnsureButtons();
            HideDataModifierBadge();
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[StatusSlot] Install failed: {ex}");
        }
    }

    /// <summary>
    /// 隐藏顶栏上 StatusSlotDataModifier 的图标。
    /// 不用 QueueFree（会导致 NTopBar.UpdateNavigation 崩溃），
    /// 改为将图标节点设为不可见。
    /// </summary>
    public static void HideDataModifierBadge()
    {
        if (ModifiersContainerField == null || TopBarModifierModelField == null)
        {
            Entry.Logger.Info("[StatusSlot] HideDataModifierBadge: top bar fields not found");
            return;
        }

        var topBar = NRun.Instance?.GlobalUi?.TopBar;
        if (topBar == null) return;

        var container = ModifiersContainerField.GetValue(topBar) as Control;
        if (container == null) return;

        int hidden = 0;
        foreach (Node child in container.GetChildren(false))
        {
            if (child is NTopBarModifier modifier)
            {
                var model = TopBarModifierModelField.GetValue(modifier);
                if (model is StatusSlotDataModifier && child is Control ctrl)
                {
                    ctrl.Visible = false;
                    hidden++;
                }
            }
        }

        if (hidden > 0)
            Entry.Logger.Info($"[StatusSlot] HideDataModifierBadge: hidden {hidden} badge(s)");
    }
}
