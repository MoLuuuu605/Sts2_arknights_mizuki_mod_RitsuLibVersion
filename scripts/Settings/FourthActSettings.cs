using Arknights_Mizuki.Scripts.Characters;
using Arknights_Mizuki.Scripts.StatusSlots;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.RunData;
using STS2RitsuLib.Settings;

namespace Arknights_Mizuki.Scripts.Settings;

internal enum SublimationEventMode
{
    Disabled,
    EventPool,
    ForceFirstUnknown
}

internal sealed class FourthActSharedRunConfig
{
    public int NormalCombatScalingPercent { get; set; } = FourthActSettings.DefaultNormalCombatScalingPercent;
    public int BossCombatScalingPercent { get; set; } = FourthActSettings.DefaultBossCombatScalingPercent;
}

internal static class FourthActSettings
{
    internal const SublimationEventMode DefaultSublimationEventMode = SublimationEventMode.ForceFirstUnknown;
    internal const bool DefaultSublimationEventAffectsOtherCharacters = false;
    internal const double DefaultNormalCombatScaling = 1.3;
    internal const double DefaultBossCombatScaling = 1.4;
    internal const int DefaultNormalCombatScalingPercent = 130;
    internal const int DefaultBossCombatScalingPercent = 140;

    internal static IModSettingsValueBinding<SublimationEventMode> SublimationMode { get; private set; } = null!;
    internal static IModSettingsValueBinding<bool> SublimationEventAffectsOtherCharacters { get; private set; } = null!;
    internal static IModSettingsValueBinding<double> NormalCombatScaling { get; private set; } = null!;
    internal static IModSettingsValueBinding<double> BossCombatScaling { get; private set; } = null!;
    internal static RunSavedData<FourthActSharedRunConfig> SharedRunConfig { get; private set; } = null!;

    internal static void RegisterRunData(RunSavedDataStore store)
    {
        SharedRunConfig = store.Register(
            "fourth_act_shared_config",
            () => new FourthActSharedRunConfig(),
            new RunSavedDataOptions
            {
                SchemaVersion = 1,
                WritePolicy = RunSavedDataWritePolicy.AlwaysWhenRegistered,
                SyncLobbyOnChange = true
            });
    }

    internal static void RegisterBindings()
    {
        SublimationMode = StatusSlotSettings.Global(
            data => data.SublimationEventMode,
            (data, value) => data.SublimationEventMode = value,
            DefaultSublimationEventMode);
        SublimationEventAffectsOtherCharacters = StatusSlotSettings.Global(
            data => data.SublimationEventAffectsOtherCharacters,
            (data, value) => data.SublimationEventAffectsOtherCharacters = value,
            DefaultSublimationEventAffectsOtherCharacters);
        NormalCombatScaling = StatusSlotSettings.Global(
            data => data.NormalCombatScaling,
            (data, value) => data.NormalCombatScaling = value,
            DefaultNormalCombatScaling);
        BossCombatScaling = StatusSlotSettings.Global(
            data => data.BossCombatScaling,
            (data, value) => data.BossCombatScaling = value,
            DefaultBossCombatScaling);
    }

    internal static SublimationEventMode CurrentSublimationMode => SublimationMode.Read();
    internal static bool DoesSublimationEventAffectOtherCharacters => SublimationEventAffectsOtherCharacters.Read();

    internal static void StageHostSharedConfig(RunSavedDataLobbyStagingEvent evt)
    {
        if (evt.IsMultiplayer && !evt.IsHost)
            return;

        FourthActSharedRunConfig config = CaptureSharedConfig();
        if (SharedRunConfig.Lobby.TryGet(evt.Lobby, out var existing) && SameScaling(existing, config))
            return;

        SharedRunConfig.Lobby.Set(evt.Lobby, config);
    }

    internal static void PrepareSharedConfig(RunSavedDataPreparingEvent evt)
    {
        FourthActSharedRunConfig config = CaptureSharedConfig();
        Normalize(config);
        SharedRunConfig.Set(evt.RunState, config);
    }

    internal static void EnsureSharedConfig(RunState runState, bool useLocalConfig)
    {
        if (SharedRunConfig.TryGet(runState, out _))
            return;
        SharedRunConfig.Set(runState, useLocalConfig ? CaptureSharedConfig() : new FourthActSharedRunConfig());
    }

    internal static SublimationEventMode GetSublimationMode(IRunState runState)
    {
        if (runState is not RunState state)
            return SublimationEventMode.Disabled;

        SublimationEventMode result = SublimationEventMode.Disabled;
        foreach (var player in state.Players)
        {
            StatusSlotPlayerConfig config = StatusSlotRunData.GetConfig(player);
            var playerMode = (SublimationEventMode)Math.Clamp(
                config.SublimationEventMode,
                (int)SublimationEventMode.Disabled,
                (int)SublimationEventMode.ForceFirstUnknown);
            bool eligibleCharacter = player.Character is Mizuki || config.SublimationEventAffectsOtherCharacters;
            if (!eligibleCharacter || playerMode == SublimationEventMode.Disabled)
                continue;
            if (playerMode == SublimationEventMode.ForceFirstUnknown)
                return playerMode;
            result = SublimationEventMode.EventPool;
        }
        return result;
    }

    internal static bool ShouldOfferSublimation(IRunState runState)
        => GetSublimationMode(runState) != SublimationEventMode.Disabled;

    internal static decimal GetMultiplayerScaling(RunState runState, RoomType roomType)
    {
        FourthActSharedRunConfig config = SharedRunConfig.Get(runState);
        int percent = roomType == RoomType.Boss
            ? config.BossCombatScalingPercent
            : config.NormalCombatScalingPercent;
        return Math.Clamp(percent, 100, 300) / 100m;
    }

    private static FourthActSharedRunConfig CaptureSharedConfig()
    {
        return new FourthActSharedRunConfig
        {
            NormalCombatScalingPercent = ToScalingPercent(NormalCombatScaling.Read()),
            BossCombatScalingPercent = ToScalingPercent(BossCombatScaling.Read())
        };
    }

    private static int ToScalingPercent(double value)
        => Math.Clamp((int)Math.Round(value * 100.0), 100, 300);

    private static void Normalize(FourthActSharedRunConfig config)
    {
        config.NormalCombatScalingPercent = Math.Clamp(config.NormalCombatScalingPercent, 100, 300);
        config.BossCombatScalingPercent = Math.Clamp(config.BossCombatScalingPercent, 100, 300);
    }

    private static bool SameScaling(FourthActSharedRunConfig left, FourthActSharedRunConfig right)
        => left.NormalCombatScalingPercent == right.NormalCombatScalingPercent &&
           left.BossCombatScalingPercent == right.BossCombatScalingPercent;
}
