using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using Arknights_Mizuki.Scripts.Settings;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace Arknights_Mizuki.Scripts.StatusSlots;

public sealed class StatusSlotPlayerConfig
{
    public bool RevelationEnabled { get; set; } = StatusSlotSettings.DefaultRevelationEnabled;
    public bool AberrationEnabled { get; set; } = StatusSlotSettings.DefaultAberrationEnabled;
    public bool SwarmCallEnabled { get; set; } = StatusSlotSettings.DefaultSwarmCallEnabled;
    public bool RevelationAffectsOtherCharacters { get; set; } = StatusSlotSettings.DefaultRevelationAffectsOtherCharacters;
    public bool AberrationAffectsOtherCharacters { get; set; } = StatusSlotSettings.DefaultAberrationAffectsOtherCharacters;
    public bool SwarmCallAffectsOtherCharacters { get; set; } = StatusSlotSettings.DefaultSwarmCallAffectsOtherCharacters;
    public int RevelationChance { get; set; } = StatusSlotRunData.ToChanceBasisPoints(StatusSlotSettings.DefaultRevelationChance);
    public int AberrationHpThreshold { get; set; } = (int)StatusSlotSettings.DefaultAberrationHpThreshold;
    public int AberrationChance { get; set; } = StatusSlotRunData.ToChanceBasisPoints(StatusSlotSettings.DefaultAberrationChance);
    public int SwarmCallChance { get; set; } = StatusSlotRunData.ToChanceBasisPoints(StatusSlotSettings.DefaultSwarmCallChance);
    public int SublimationEventMode { get; set; } = (int)FourthActSettings.DefaultSublimationEventMode;
    public bool SublimationEventAffectsOtherCharacters { get; set; } = FourthActSettings.DefaultSublimationEventAffectsOtherCharacters;

    public bool HasSameValues(StatusSlotPlayerConfig other)
    {
        return RevelationEnabled == other.RevelationEnabled &&
               AberrationEnabled == other.AberrationEnabled &&
               SwarmCallEnabled == other.SwarmCallEnabled &&
               RevelationAffectsOtherCharacters == other.RevelationAffectsOtherCharacters &&
               AberrationAffectsOtherCharacters == other.AberrationAffectsOtherCharacters &&
               SwarmCallAffectsOtherCharacters == other.SwarmCallAffectsOtherCharacters &&
               RevelationChance == other.RevelationChance &&
               AberrationHpThreshold == other.AberrationHpThreshold &&
               AberrationChance == other.AberrationChance &&
               SwarmCallChance == other.SwarmCallChance &&
               SublimationEventMode == other.SublimationEventMode &&
               SublimationEventAffectsOtherCharacters == other.SublimationEventAffectsOtherCharacters;
    }
}

public sealed class StatusSlotPlayerState
{
    public string RevelationKey { get; set; } = "";
    public int RevelationCharges { get; set; }
    public string AberrationKey { get; set; } = "";
    public int AberrationCharges { get; set; }
    public string SwarmCallKey { get; set; } = "";
    public int SwarmCallCharges { get; set; }

    public bool ColumbiaPendingReward { get; set; }
    public int EchoModificationGoldGained { get; set; }
    public int EchoModificationGoldOnAcquire { get; set; }
    public int FleshDistortionLostHp { get; set; }
    public int EchoLongRoadCombatCount { get; set; }
    public int DamageTakenThisCombat { get; set; }
    public int DamageEventsThisCombat { get; set; }

    public int LastActIndex { get; set; } = -1;
    public int LastProcessedRoomCount { get; set; } = -1;
    public int LastAberrationCombatRoomCount { get; set; } = -1;
    public int Revision { get; set; }

    public string GetKey(StatusSlotType type) => type switch
    {
        StatusSlotType.Revelation => RevelationKey,
        StatusSlotType.Aberration => AberrationKey,
        StatusSlotType.SwarmCall => SwarmCallKey,
        _ => ""
    };

    public int GetCharges(StatusSlotType type) => type switch
    {
        StatusSlotType.Revelation => RevelationCharges,
        StatusSlotType.Aberration => AberrationCharges,
        StatusSlotType.SwarmCall => SwarmCallCharges,
        _ => 0
    };

    public void SetSlot(StatusSlotType type, string key, int charges)
    {
        switch (type)
        {
            case StatusSlotType.Revelation:
                RevelationKey = key;
                RevelationCharges = charges;
                break;
            case StatusSlotType.Aberration:
                AberrationKey = key;
                AberrationCharges = charges;
                break;
            case StatusSlotType.SwarmCall:
                SwarmCallKey = key;
                SwarmCallCharges = charges;
                break;
        }
    }

    public void ClearSlot(StatusSlotType type) => SetSlot(type, "", 0);
}

internal static class StatusSlotRunData
{
    internal static PlayerRunSavedData<StatusSlotPlayerConfig> Configs { get; private set; } = null!;
    internal static PlayerRunSavedData<StatusSlotPlayerState> States { get; private set; } = null!;

    internal static void Register()
    {
        using (RitsuLibFramework.BeginModDataRegistration(Entry.ModId))
        {
            var store = RitsuLibFramework.GetRunSavedDataStore(Entry.ModId);
            Configs = store.RegisterPerPlayer(
                "status_slot_config",
                CreateDefaultConfig,
                new RunSavedDataOptions
                {
                    SchemaVersion = 1,
                    WritePolicy = RunSavedDataWritePolicy.AlwaysWhenRegistered,
                    SyncLobbyOnChange = true
                });
            States = store.RegisterPerPlayer(
                "status_slot_state",
                () => new StatusSlotPlayerState(),
                new RunSavedDataOptions
                {
                    SchemaVersion = 1,
                    WritePolicy = RunSavedDataWritePolicy.AlwaysWhenRegistered
                });
            FourthActSettings.RegisterRunData(store);
        }
    }

    internal static void StageLocalConfig(RunSavedDataLobbyStagingEvent evt)
    {
        ulong localNetId = evt.Lobby.NetService.NetId;
        StatusSlotPlayerConfig localConfig = CaptureLocalConfig();
        FourthActSettings.StageHostSharedConfig(evt);
        if (Configs.Lobby.TryGet(evt.Lobby, localNetId, out var existing) &&
            existing.HasSameValues(localConfig))
        {
            return;
        }

        Configs.Lobby.Set(evt.Lobby, localNetId, localConfig);
        Entry.Logger.Info($"[StatusSlot] Staged local config for player {localNetId}");
    }

    internal static void PrepareRunData(RunSavedDataPreparingEvent evt)
    {
        if (evt.IsMultiplayer && RunManager.Instance.NetService is not NetHostGameService)
            return;

        FourthActSettings.PrepareSharedConfig(evt);

        Player? localPlayer = LocalContext.GetMe(evt.RunState);
        StatusSlotPlayerConfig hostConfig = CaptureLocalConfig();
        foreach (Player player in evt.RunState.Players)
        {
            StatusSlotPlayerConfig config;
            if (ReferenceEquals(player, localPlayer))
            {
                config = hostConfig;
            }
            else if (!Configs.TryGet(evt.RunState, player.NetId, out config))
            {
                config = CreateDefaultConfig();
            }

            if (evt.IsMultiplayer)
                CopyMechanismConfig(config, hostConfig);

            Normalize(config);
            Configs.Set(evt.RunState, player.NetId, config);

            States.Modify(evt.RunState, player.NetId, _ => { });
        }

        if (evt.IsMultiplayer)
        {
            Entry.Logger.Info(
                $"[StatusSlot] Applied host mechanism config to {evt.RunState.Players.Count} players: " +
                $"revelation={hostConfig.RevelationEnabled}/{hostConfig.RevelationChance}, " +
                $"aberration={hostConfig.AberrationEnabled}/{hostConfig.AberrationHpThreshold}/{hostConfig.AberrationChance}, " +
                $"swarm={hostConfig.SwarmCallEnabled}/{hostConfig.SwarmCallChance}");
        }
    }

    internal static void EnsurePlayers(RunState runState)
    {
        Player? localPlayer = LocalContext.GetMe(runState);
        bool useLocalConfig = RunManager.Instance.IsSingleplayerOrFakeMultiplayer;
        FourthActSettings.EnsureSharedConfig(runState, useLocalConfig);
        foreach (Player player in runState.Players)
        {
            if (!Configs.TryGet(runState, player.NetId, out _))
            {
                Configs.Set(
                    runState,
                    player.NetId,
                    useLocalConfig && (ReferenceEquals(player, localPlayer) || runState.Players.Count == 1)
                        ? CaptureLocalConfig()
                        : CreateDefaultConfig());
            }

            States.Modify(runState, player.NetId, _ => { });
        }
    }

    internal static StatusSlotPlayerConfig GetConfig(Player player)
    {
        return Configs.Get((RunState)player.RunState, player.NetId);
    }

    internal static StatusSlotPlayerState GetState(Player player)
    {
        return States.Get((RunState)player.RunState, player.NetId);
    }

    internal static StatusSlotPlayerConfig CaptureLocalConfig()
    {
        return new StatusSlotPlayerConfig
        {
            RevelationEnabled = StatusSlotSettings.IsRevelationEnabled,
            AberrationEnabled = StatusSlotSettings.IsAberrationEnabled,
            SwarmCallEnabled = StatusSlotSettings.IsSwarmCallEnabled,
            RevelationAffectsOtherCharacters = StatusSlotSettings.DoesRevelationAffectOtherCharacters,
            AberrationAffectsOtherCharacters = StatusSlotSettings.DoesAberrationAffectOtherCharacters,
            SwarmCallAffectsOtherCharacters = StatusSlotSettings.DoesSwarmCallAffectOtherCharacters,
            RevelationChance = ToChanceBasisPoints(StatusSlotSettings.RevelationChanceValue),
            AberrationHpThreshold = Math.Clamp(StatusSlotSettings.AberrationHpThresholdValue, 1, 999),
            AberrationChance = ToChanceBasisPoints(StatusSlotSettings.AberrationChanceValue),
            SwarmCallChance = ToChanceBasisPoints(StatusSlotSettings.SwarmCallChanceValue),
            SublimationEventMode = (int)FourthActSettings.CurrentSublimationMode,
            SublimationEventAffectsOtherCharacters = FourthActSettings.DoesSublimationEventAffectOtherCharacters
        };
    }

    internal static StatusSlotPlayerConfig CreateDefaultConfig() => new();

    internal static int ToChanceBasisPoints(double chance)
    {
        return Math.Clamp((int)Math.Round(chance * 10000.0), 0, 10000);
    }

    private static void CopyMechanismConfig(
        StatusSlotPlayerConfig target,
        StatusSlotPlayerConfig source)
    {
        target.RevelationEnabled = source.RevelationEnabled;
        target.AberrationEnabled = source.AberrationEnabled;
        target.SwarmCallEnabled = source.SwarmCallEnabled;
        target.RevelationAffectsOtherCharacters = source.RevelationAffectsOtherCharacters;
        target.AberrationAffectsOtherCharacters = source.AberrationAffectsOtherCharacters;
        target.SwarmCallAffectsOtherCharacters = source.SwarmCallAffectsOtherCharacters;
        target.RevelationChance = source.RevelationChance;
        target.AberrationHpThreshold = source.AberrationHpThreshold;
        target.AberrationChance = source.AberrationChance;
        target.SwarmCallChance = source.SwarmCallChance;
    }

    private static void Normalize(StatusSlotPlayerConfig config)
    {
        config.RevelationChance = Math.Clamp(config.RevelationChance, 0, 10000);
        config.AberrationHpThreshold = Math.Clamp(config.AberrationHpThreshold, 1, 999);
        config.AberrationChance = Math.Clamp(config.AberrationChance, 0, 10000);
        config.SwarmCallChance = Math.Clamp(config.SwarmCallChance, 0, 10000);
    }
}
