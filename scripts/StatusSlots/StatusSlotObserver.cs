using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 状态栏位事件观察者。处理大厅配置贡献、跑局数据初始化和房间 UI 生命周期。
/// 战斗钩子由 StatusSlotDataModifier 承载，持久状态由 RitsuLib 按玩家跑局数据保存。
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
                case ActEnteredEvent e:
                    OnActEntered(e);
                    break;
                case RoomExitedEvent e:
                    OnRoomExited(e);
                    break;
                case RunSavedDataLobbyStagingEvent e:
                    StatusSlotRunData.StageLocalConfig(e);
                    break;
                case RunSavedDataPreparingEvent e:
                    StatusSlotRunData.PrepareRunData(e);
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
        if (evt.RunState.CurrentMapPoint is { PointType: not MapPointType.Ancient })
            StatusSlotManager.AttachModifier(evt.RunState);
        StatusSlotFrame.EnsureButtons();
        StatusSlotManager.RefreshUI();
    }

    private void OnRoomEntering(RoomEnteringEvent evt)
    {
        if (evt.RunState is not RunState runState)
            return;

        // Fallback for legacy or externally constructed runs. Normal runs attach at
        // RunStarted/RunLoaded so the first combat's hook snapshot includes this modifier.
        var currentMapPoint = runState.CurrentMapPoint;
        if (currentMapPoint != null && currentMapPoint.PointType != MapPointType.Ancient)
        {
            StatusSlotManager.EnsureData(runState);
            StatusSlotManager.AttachModifier(runState);
        }
    }

    private void OnRoomEntered(RoomEnteredEvent evt)
    {
        Entry.Logger.Info(
            $"[StatusSlot][RoomEntered] floor={evt.RunState.TotalFloor} " +
            $"roomStack={evt.RunState.CurrentRoomCount} type={evt.Room.RoomType}");
        TaskHelper.RunSafely(SettleRoomEnteredAsync(evt));
        StatusSlotFrame.EnsureButtons();
        StatusSlotPatches.HideDataModifierBadge();
        StatusSlotManager.RefreshUI();
    }

    private static async Task SettleRoomEnteredAsync(RoomEnteredEvent evt)
    {
        if (evt.RunState is not RunState runState)
            return;

        StatusSlotManager.EnsureData(runState);
        await StatusSlotManager.SettleRoomEnteredAsync(runState, evt.Room);
        await StatusSlotManager.SettleEchoModificationRoomEnteredAsync(runState, evt.Room);
    }

    private static void OnActEntered(ActEnteredEvent evt)
    {
        if (evt.RunState is not RunState runState)
            return;

        StatusSlotManager.EnsureData(runState);
        TaskHelper.RunSafely(StatusSlotManager.SettleActEnteredAsync(runState, evt.CurrentActIndex));
    }

    private void OnRoomExited(RoomExitedEvent evt)
    {
        RunState? runState = StatusSlotManager.GetRunState();
        if (runState == null)
            return;

        StatusSlotManager.EnsureData(runState);
        StatusSlotManager.AttachModifier(runState);
    }
}
