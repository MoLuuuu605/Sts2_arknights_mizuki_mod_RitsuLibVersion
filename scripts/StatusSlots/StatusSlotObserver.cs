using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 状态栏位事件观察者。Singleton，实现 ILifecycleObserver。
/// 处理非战斗事件：开局/读档初始化、房间进入。
/// 战斗内事件由 StatusSlotDataModifier 的 override 方法处理。
/// </summary>
public sealed class StatusSlotObserver : ILifecycleObserver
{
    public static readonly StatusSlotObserver Instance = new();

    private StatusSlotObserver() { }

    public void OnEvent(IFrameworkLifecycleEvent evt)
    {
        try
        {
            switch (evt)
            {
                case RunStartedEvent e:
                    OnRunStarted(e);
                    break;
                case RunLoadedEvent e:
                    OnRunLoaded(e);
                    break;
                case RoomEnteringEvent e:
                    OnRoomEntering(e);
                    break;
                case RoomEnteredEvent e:
                    OnRoomEntered(e);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Entry.Logger.Error($"[StatusSlot] Observer error on {evt.GetType().Name}: {ex}");
        }
    }

    private void OnRunStarted(RunStartedEvent evt)
    {
        Entry.Logger.Info("[StatusSlot] RunStarted, initializing data");
        StatusSlotManager.EnsureData(evt.RunState);
        StatusSlotManager.RefreshUI();
    }

    private void OnRunLoaded(RunLoadedEvent evt)
    {
        Entry.Logger.Info("[StatusSlot] RunLoaded, restoring data");
        StatusSlotManager.EnsureData(evt.RunState);
        StatusSlotFrame.EnsureButtons();
        StatusSlotManager.RefreshUI();
    }

    private void OnRoomEntering(RoomEnteringEvent evt)
    {
        if (evt.RunState is not RunState runState)
            return;

        // ── 延迟挂载 Modifier ──
        // 新局开始时 StatusSlotDataModifier 只创建未挂载（避免影响 Neow 选项生成）。
        // 进入非 Ancient 房间时才挂到 RunState.Modifiers，让存档系统接管。
        var currentMapPoint = runState.CurrentMapPoint;
        if (currentMapPoint != null && currentMapPoint.PointType != MapPointType.Ancient)
        {
            StatusSlotManager.EnsureData(runState);
            StatusSlotManager.AttachModifier(runState);
        }
    }

    private void OnRoomEntered(RoomEnteredEvent evt)
    {
        StatusSlotFrame.EnsureButtons();
        StatusSlotPatches.HideDataModifierBadge();
        StatusSlotManager.RefreshUI();
    }
}
