using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 顶栏三个框位：启示 / 排异反应 / 回响。
/// 仿照 HextechEnemyHexCollapseView 的做法：地图按钮左侧深色底框，
/// 鼠标悬浮时弹出提示面板（挂进 HoverTipsContainer 置为第 0 个子节点）。
/// 通过 SetContent(index, iconPath, title, description) 更新显示。
/// </summary>
internal static class StatusSlotFrame
{
    private const string ContainerName = "ArknightsStatusSlotContainer";
    private const string MapButtonTypeName = "NTopBarMapButton";
    private const string DeckButtonTypeName = "NTopBarDeckButton";
    private const string TipBgPath = "res://Arknights_Mizuki/images/ui/hovertip.png";
    private static readonly Vector2 FallbackButtonSize = new(52f, 52f);

    private static HBoxContainer? _container;
    private static readonly SlotState[] _slots = new SlotState[3];
    private static readonly string[] DefaultTitles =
    {
        StatusSlotI18n.GetSlotName(StatusSlotType.Revelation),
        StatusSlotI18n.GetSlotName(StatusSlotType.Aberration),
        StatusSlotI18n.GetSlotName(StatusSlotType.SwarmCall)
    };
    private static readonly string[] DefaultDescs =
    {
        StatusSlotI18n.GetSlotEmpty(StatusSlotType.Revelation),
        StatusSlotI18n.GetSlotEmpty(StatusSlotType.Aberration),
        StatusSlotI18n.GetSlotEmpty(StatusSlotType.SwarmCall)
    };

    private struct SlotState
    {
        public Button? Button;
        public TextureRect? IconRect;
        public Control? TipRoot;
        public Control? TipBg;
        public Label? TipTitle;
        public Label? TipDesc;
        public bool Hovered;
        public string Title;
        public string Description;
    }

    /// <summary>
    /// 在顶栏创建三个框位。已存在则跳过。
    /// </summary>
    internal static void EnsureButtons()
    {
        if (_container != null && GodotObject.IsInstanceValid(_container))
            return;

        try
        {
            var mapButton = FindMapButton();
            if (mapButton == null) return;
            var parent = mapButton.GetParent();
            if (parent == null) return;

            Vector2 iconSize = mapButton is Control mapControl ? mapControl.Size : Vector2.Zero;
            if (iconSize.X < 8f || iconSize.Y < 8f)
                iconSize = FallbackButtonSize;

            var container = new HBoxContainer
            {
                Name = ContainerName,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            container.AddThemeConstantOverride("separation", 4);

            for (int i = 0; i < 3; i++)
            {
                ref var slot = ref _slots[i];
                slot.Title = DefaultTitles[i];
                slot.Description = DefaultDescs[i];
                CreateSlot(ref slot, i, iconSize);
                container.AddChild(slot.Button!);
                // 根据开关设置初始可见性
                slot.Button!.Visible = IsSlotEnabled(i);
            }

            parent.AddChild(container);
            parent.MoveChild(container, mapButton.GetIndex());

            _container = container;

            // 延迟创建提示面板（需要 HoverTipsContainer 就绪）
            for (int i = 0; i < 3; i++)
            {
                EnsureTipPanel(i);
            }

            Entry.Logger.Info("[StatusSlot] 3 frames created");
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[StatusSlot] EnsureButtons failed: {ex}");
        }
    }

    /// <summary>
    /// 更新某个框位的显示内容。
    /// iconPath 为 null 或空时只显示空框；否则加载图片显示。
    /// </summary>
    internal static void SetContent(int index, string? iconPath, string title, string description)
    {
        if (index < 0 || index >= 3) return;
        ref var slot = ref _slots[index];
        slot.Title = title;
        slot.Description = description;

        if (slot.IconRect != null && GodotObject.IsInstanceValid(slot.IconRect))
        {
            if (!string.IsNullOrEmpty(iconPath))
            {
                var tex = LoadTexture(iconPath);

                slot.IconRect.Texture = tex;
            }
            else
            {
                slot.IconRect.Texture = null;
            }
        }

        if (slot.Hovered)
            UpdateTipPanel(index);
    }

    internal static void SetSlotVisible(int index, bool visible)
    {
        if (index < 0 || index >= 3) return;
        ref var slot = ref _slots[index];

        if (slot.Button != null && GodotObject.IsInstanceValid(slot.Button))
            slot.Button.Visible = visible;

        if (!visible)
        {
            slot.Hovered = false;
            if (slot.TipRoot != null && GodotObject.IsInstanceValid(slot.TipRoot))
                slot.TipRoot.Visible = false;
        }
    }

    /// <summary>
    /// 更新某个框位的显示内容（直接传 Texture2D）。
    /// </summary>
    internal static void SetContent(int index, Texture2D? icon, string title, string description)
    {
        if (index < 0 || index >= 3) return;
        ref var slot = ref _slots[index];
        slot.Title = title;
        slot.Description = description;

        if (slot.IconRect != null && GodotObject.IsInstanceValid(slot.IconRect))
        {
            slot.IconRect.Texture = icon;
        }

        if (slot.Hovered)
            UpdateTipPanel(index);
    }

    /// <summary>
    /// 移除所有框位和提示面板。
    /// </summary>
    internal static void Remove()
    {
        for (int i = 0; i < 3; i++)
        {
            ref var slot = ref _slots[i];
            QueueFreeIfValid(slot.Button);
            QueueFreeIfValid(slot.TipRoot);
            slot.Button = null;
            slot.IconRect = null;
            slot.TipRoot = null;
            slot.TipBg = null;
            slot.TipTitle = null;
            slot.TipDesc = null;
            slot.Hovered = false;
        }
        QueueFreeIfValid(_container);
        _container = null;
    }

    // ─── 内部 ──────────────────────────────────────────────

    /// <summary>
    /// 根据开关设置判断某个槽位是否启用。
    /// </summary>
    private static bool IsSlotEnabled(int index) => index switch
    {
        0 => StatusSlotManager.IsSlotVisibleForLocalPlayer(StatusSlotType.Revelation),
        1 => StatusSlotManager.IsSlotVisibleForLocalPlayer(StatusSlotType.Aberration),
        2 => StatusSlotManager.IsSlotVisibleForLocalPlayer(StatusSlotType.SwarmCall),
        _ => true
    };

    private static void CreateSlot(ref SlotState slot, int index, Vector2 iconSize)
    {
        var size = iconSize * 0.75f;

        var button = new Button
        {
            Name = $"ArknightsStatusSlot_{index}",
            FocusMode = Control.FocusModeEnum.None,
            MouseFilter = Control.MouseFilterEnum.Stop,
            CustomMinimumSize = size,
        };
        button.SizeFlagsVertical = (Control.SizeFlags)4; // ShrinkCenter
        button.AddThemeStyleboxOverride("normal", CreateButtonStyle(0.62f));
        button.AddThemeStyleboxOverride("hover", CreateButtonStyle(0.82f));
        button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(0.9f));
        button.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());

        var icon = new TextureRect
        {
            Name = "SlotIcon",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        };
        icon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        icon.OffsetLeft = 4f;
        icon.OffsetTop = 4f;
        icon.OffsetRight = -4f;
        icon.OffsetBottom = -4f;
        button.AddChild(icon);

        int capturedIndex = index;
        button.MouseEntered += () => { _slots[capturedIndex].Hovered = true; UpdateTipPanel(capturedIndex); };
        button.MouseExited += () => { _slots[capturedIndex].Hovered = false; UpdateTipPanel(capturedIndex); };

        slot.Button = button;
        slot.IconRect = icon;
    }

    private static void EnsureTipPanel(int index)
    {
        ref var slot = ref _slots[index];
        if (slot.TipRoot != null && GodotObject.IsInstanceValid(slot.TipRoot))
            return;

        QueueFreeIfValid(slot.TipRoot);

        // 根节点：Control（自由定位）
        var root = new Control
        {
            Name = $"ArknightsStatusSlotTip_{index}",
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(228, 160),
        };
        root.SetAnchorsPreset(Control.LayoutPreset.TopLeft);    
        // 背景图片：TextureRect 自由缩放铺满
        var bg = new Panel
        {
            Name = "TipBg",
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        var styleBox = new StyleBoxTexture
        {
            Texture = LoadTexture(TipBgPath),
            ExpandMarginLeft = 20f,   // 九宫格左侧不拉伸区域
            ExpandMarginRight = 20f,
            ExpandMarginTop = 20f,
            ExpandMarginBottom = 20f,
        };
        bg.AddThemeStyleboxOverride("panel", styleBox);
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(bg);

        // 内容容器：margin + vbox，放在背景之上
        var margin = new MarginContainer
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 14);
        margin.AddThemeConstantOverride("margin_right", 14);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_bottom", 30);

        var vbox = new VBoxContainer
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        vbox.AddThemeConstantOverride("separation", 3);

        var title = new Label
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0.4f)); // 金色
        title.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.9f));
        title.AddThemeConstantOverride("outline_size", 3);

        var desc = new Label
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Left,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        desc.AddThemeFontSizeOverride("font_size", 18);
        desc.AddThemeColorOverride("font_color", new Color(0.78f, 0.76f, 0.72f));
        desc.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.85f));
        desc.AddThemeConstantOverride("outline_size", 3);
        desc.CustomMinimumSize = new Vector2(200, 20);

        vbox.AddChild(title);
        vbox.AddChild(desc);
        margin.AddChild(vbox);
        root.AddChild(margin);

        // 挂进 HoverTipsContainer，置为第 0 个子节点（参考 Hex 做法）
        var host = NGame.Instance?.HoverTipsContainer;
        if (host == null)
        {
            Entry.Logger.Error($"[StatusSlot] HoverTipsContainer is null! TipPanel {index} not attached.");
        }
        else
        {
            host.AddChild(root);
            host.MoveChild(root, 0);
        }

        slot.TipRoot = root;
        slot.TipBg = bg;
        slot.TipTitle = title;
        slot.TipDesc = desc;
    }

    private static void UpdateTipPanel(int index)
    {
        ref var slot = ref _slots[index];
        if (slot.TipRoot == null || !GodotObject.IsInstanceValid(slot.TipRoot))
            return;

        if (slot.Hovered)
        {
            if (slot.TipTitle != null && GodotObject.IsInstanceValid(slot.TipTitle))
                slot.TipTitle.Text = slot.Title;
            if (slot.TipDesc != null && GodotObject.IsInstanceValid(slot.TipDesc))
                slot.TipDesc.Text = slot.Description;

            slot.TipRoot.Visible = true;
            PositionTipPanel(index);
        }
        else
        {
            slot.TipRoot.Visible = false;
        }
    }

    private static void PositionTipPanel(int index)
    {
        ref var slot = ref _slots[index];
        if (slot.TipRoot == null || slot.Button == null
            || !GodotObject.IsInstanceValid(slot.TipRoot) || !GodotObject.IsInstanceValid(slot.Button))
            return;

        var button = slot.Button;
        var root = slot.TipRoot;

        // 等一帧让面板算出尺寸
        Callable.From(() =>
        {
            if (root == null || !GodotObject.IsInstanceValid(root)) return;

            Rect2 buttonRect = button!.GetGlobalRect();
            float panelWidth = root.Size.X > 8f ? root.Size.X : 200f;
            float screenWidth = root.GetViewportRect().Size.X;

            float x = buttonRect.Position.X;
            if (x + panelWidth > screenWidth - 8f)
                x = screenWidth - 8f - panelWidth;
            if (x < 8f)
                x = 8f;

            root.GlobalPosition = new Vector2(x, buttonRect.End.Y + 6f);
        }).CallDeferred();
    }

    private static Texture2D? LoadTexture(string path)
    {
        try
        {
            if (ResourceLoader.Exists(path))
                return ResourceLoader.Load<Texture2D>(path);
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[StatusSlot] LoadTexture failed for {path}: {ex.Message}");
        }
        return null;
    }

    private static void QueueFreeIfValid(Node? node)
    {
        if (node != null && GodotObject.IsInstanceValid(node))
            node.QueueFree();
    }

    private static Node? FindMapButton()
    {
        var topBar = NRun.Instance?.GlobalUi?.TopBar;
        if (topBar == null) return null;
        var btn = FindDescendantByTypeName(topBar, MapButtonTypeName);
        return btn ?? FindDescendantByTypeName(topBar, DeckButtonTypeName);
    }

    private static Node? FindDescendantByTypeName(Node node, string typeName)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child.GetType().Name == typeName) return child;
            var found = FindDescendantByTypeName(child, typeName);
            if (found != null) return found;
        }
        return null;
    }

    private static StyleBoxFlat CreateButtonStyle(float bgAlpha)
    {
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.06f, 0.09f, bgAlpha),
            BorderColor = new Color(0.42f, 0.48f, 0.6f, 0.42f),
        };
        style.SetBorderWidthAll(1);
        style.SetCornerRadiusAll(8);
        return style;
    }
}
