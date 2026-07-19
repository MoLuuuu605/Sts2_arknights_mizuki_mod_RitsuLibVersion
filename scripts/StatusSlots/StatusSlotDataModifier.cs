using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// Hook carrier for StatusSlot gameplay. Persistent state lives in the per-player
/// RitsuLib run-data slots; this model only participates in vanilla awaited hooks.
/// </summary>
public sealed class StatusSlotDataModifier : ModifierModel
{
    private static readonly HashSet<ulong> EchoWitherBonusInProgress = new();

    // Legacy fields are retained so saves from the former shared-state implementation
    // can be loaded and migrated once into the per-player run-data slots.
    [SavedProperty] public bool PerPlayerMigrationCompleted { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public string RevelationKey { get; set; } = "";
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int RevelationCharges { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public string AberrationKey { get; set; } = "";
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int AberrationCharges { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public string SwarmCallKey { get; set; } = "";
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int SwarmCallCharges { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public bool ColumbiaPendingReward { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int EchoModificationGoldGained { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int EchoModificationGoldOnAcquire { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int LastActIndex { get; set; } = -1;
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int FleshDistortionLostHp { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int EchoLongRoadCombatCount { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int DamageTakenThisCombat { get; set; }
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)] public int DamageEventsThisCombat { get; set; }
    [SavedProperty] public bool RunSettingsCaptured { get; set; }
    [SavedProperty] public bool RevelationEnabledForRun { get; set; } = StatusSlotSettings.DefaultRevelationEnabled;
    [SavedProperty] public bool AberrationEnabledForRun { get; set; } = StatusSlotSettings.DefaultAberrationEnabled;
    [SavedProperty] public bool SwarmCallEnabledForRun { get; set; } = StatusSlotSettings.DefaultSwarmCallEnabled;
    [SavedProperty] public bool RevelationAffectsOtherCharactersForRun { get; set; } = StatusSlotSettings.DefaultRevelationAffectsOtherCharacters;
    [SavedProperty] public bool AberrationAffectsOtherCharactersForRun { get; set; } = StatusSlotSettings.DefaultAberrationAffectsOtherCharacters;
    [SavedProperty] public bool SwarmCallAffectsOtherCharactersForRun { get; set; } = StatusSlotSettings.DefaultSwarmCallAffectsOtherCharacters;
    [SavedProperty] public int SwarmCallChanceForRun { get; set; } = StatusSlotRunData.ToChanceBasisPoints(StatusSlotSettings.DefaultSwarmCallChance);
    [SavedProperty] public int RevelationChanceForRun { get; set; } = StatusSlotRunData.ToChanceBasisPoints(StatusSlotSettings.DefaultRevelationChance);
    [SavedProperty] public int AberrationHpThresholdForRun { get; set; } = (int)StatusSlotSettings.DefaultAberrationHpThreshold;
    [SavedProperty] public int AberrationChanceForRun { get; set; } = StatusSlotRunData.ToChanceBasisPoints(StatusSlotSettings.DefaultAberrationChance);

    public override bool ShouldReceiveCombatHooks => true;

    internal void MigrateLegacyState(RunState runState)
    {
        if (PerPlayerMigrationCompleted)
            return;

        bool hasLegacyState = !string.IsNullOrEmpty(RevelationKey) ||
                              !string.IsNullOrEmpty(AberrationKey) ||
                              !string.IsNullOrEmpty(SwarmCallKey);
        if (hasLegacyState)
        {
            MigrateSlot(runState, StatusSlotType.Revelation, RevelationKey, RevelationCharges);
            MigrateSlot(runState, StatusSlotType.Aberration, AberrationKey, AberrationCharges);
            MigrateSlot(runState, StatusSlotType.SwarmCall, SwarmCallKey, SwarmCallCharges);
            Entry.Logger.Info("[StatusSlot] Migrated legacy shared slot state to per-player run data");
        }

        PerPlayerMigrationCompleted = true;
        RevelationKey = AberrationKey = SwarmCallKey = "";
        RevelationCharges = AberrationCharges = SwarmCallCharges = 0;
    }

    private void MigrateSlot(RunState runState, StatusSlotType type, string key, int charges)
    {
        Player? player = StatusSlotManager.GetOrderedPlayers()
            .FirstOrDefault(candidate => StatusSlotManager.IsSlotEnabled(candidate, type));
        if (player == null)
            return;

        StatusSlotPlayerState state = StatusSlotManager.GetState(player);
        if (!string.IsNullOrEmpty(key) && charges > 0 && string.IsNullOrEmpty(state.GetKey(type)))
            state.SetSlot(type, key, charges);

        state.LastActIndex = LastActIndex;
        state.LastProcessedRoomCount = runState.CurrentRoomCount;
        switch (type)
        {
            case StatusSlotType.Revelation:
                state.ColumbiaPendingReward = ColumbiaPendingReward;
                break;
            case StatusSlotType.Aberration:
                state.FleshDistortionLostHp = FleshDistortionLostHp;
                state.DamageTakenThisCombat = DamageTakenThisCombat;
                state.DamageEventsThisCombat = DamageEventsThisCombat;
                break;
            case StatusSlotType.SwarmCall:
                state.EchoModificationGoldGained = EchoModificationGoldGained;
                state.EchoModificationGoldOnAcquire = EchoModificationGoldOnAcquire;
                state.EchoLongRoadCombatCount = EchoLongRoadCombatCount;
                break;
        }
        state.Revision++;
        StatusSlotManager.CommitState(player, state);
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        await base.AfterRoomEntered(room);
        RunState? runState = StatusSlotManager.GetRunState();
        if (runState == null || runState.CurrentRoomCount <= 0 ||
            runState.CurrentMapPoint?.PointType == MapPointType.Ancient)
        {
            return;
        }

        try
        {
            // Nested queue actions can cross PreCombatSetup/PlayPhase differently per peer.
            // Run data and AuthorityRandom inputs are synchronized, so settle inline on every peer.
            StatusSlotRoomAction action = StatusSlotManager.BuildRoomAction(room);
            await StatusSlotManager.ApplyRoomActionAsync(action, new ThrowingPlayerChoiceContext());
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[StatusSlot] Room settlement failed; continuing room entry: {ex}");
        }
        finally
        {
            StatusSlotManager.RefreshUI();
        }
    }

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();

        // On the first room of a run, BeforeCombatStart can precede AfterRoomEntered.
        // Settle the combat room first so newly rolled effects are mounted this combat.
        StatusSlotRoomAction roomAction = StatusSlotManager.BuildRoomAction(RoomType.Monster);
        await StatusSlotManager.ApplyRoomActionAsync(
            roomAction,
            new ThrowingPlayerChoiceContext());

        var players = StatusSlotManager.GetOrderedPlayers().ToList();
        foreach (Player player in players)
        {
            StatusSlotPlayerState state = StatusSlotManager.GetState(player);
            state.DamageTakenThisCombat = 0;
            state.DamageEventsThisCombat = 0;
            StatusSlotManager.CommitState(player, state);
        }

        ICombatState? combatState = players.Select(player => player.Creature.CombatState).FirstOrDefault(state => state != null);
        if (combatState == null)
            return;

        var choiceContext = new ThrowingPlayerChoiceContext();
        foreach (Player player in players)
        {
            string revelation = StatusSlotManager.GetEffectKey(player, StatusSlotType.Revelation);
            string aberration = StatusSlotManager.GetEffectKey(player, StatusSlotType.Aberration);
            string swarmCall = StatusSlotManager.GetEffectKey(player, StatusSlotType.SwarmCall);
            Entry.Logger.Info(
                $"[StatusSlot][Combat] Mounting player={player.NetId} " +
                $"rev={revelation} aber={aberration} swarm={swarmCall}");

            if (Has(player, StatusSlotType.Revelation, "ursus_roar"))
            {
                foreach (Creature enemy in combatState.Enemies)
                    if (enemy.IsAlive)
                        await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 2, player.Creature, null, false);
            }

            if (Has(player, StatusSlotType.Revelation, "leithanien_elegance"))
                await PowerCmd.Apply<PlatingPower>(choiceContext, player.Creature, 8, player.Creature, null, false);

            if (Has(player, StatusSlotType.Revelation, "yan_erosion"))
                await PowerCmd.Apply<YanErosionPower>(choiceContext, player.Creature, 1, player.Creature, null, false);

            if (Has(player, StatusSlotType.Revelation, "sami_fortitude"))
                await PowerCmd.Apply<SamiFortitudePower>(choiceContext, player.Creature, 1, player.Creature, null, false);

            if (Has(player, StatusSlotType.Revelation, "victoria_glory"))
                await PowerCmd.Apply<VictoriaGloryPower>(choiceContext, player.Creature, 1, player.Creature, null, false);

            if (Has(player, StatusSlotType.Aberration, "neurodegeneration"))
                await PowerCmd.Apply<RingingPower>(choiceContext, player.Creature, 1, player.Creature, null, false);

            if (Has(player, StatusSlotType.Aberration, "hematopoietic_disorder"))
                await PowerCmd.Apply<HematopoieticDisorderPower>(choiceContext, player.Creature, 1, player.Creature, null, false);

            if (Has(player, StatusSlotType.Aberration, "focus_disorder"))
                await PowerCmd.Apply<FocusDisorderPower>(choiceContext, player.Creature, 1, player.Creature, null, false);

            // Each Echo: Supply owner contributes 4 Plating, so multiple owners stack.
            if (Has(player, StatusSlotType.SwarmCall, "echo_supply"))
            {
                foreach (Creature enemy in combatState.Enemies)
                    if (enemy.IsAlive)
                        await PowerCmd.Apply<PlatingPower>(choiceContext, enemy, 4, player.Creature, null, false);
            }

            if (Has(player, StatusSlotType.SwarmCall, "echo_allme"))
            {
                await PowerCmd.Apply<EchoAllMeDamagePower>(choiceContext, player.Creature, 5, player.Creature, null, false);
                await PowerCmd.Apply<EchoAllMeEnergyPower>(choiceContext, player.Creature, 10, player.Creature, null, false);
            }

            if (Has(player, StatusSlotType.SwarmCall, "echo_strife"))
                await PowerCmd.Apply<EchoStrifePower>(choiceContext, player.Creature, 1, player.Creature, null, false);

            if (Has(player, StatusSlotType.SwarmCall, "echo_long_road"))
            {
                await PowerCmd.Apply<EchoLongRoadVulnerabilityPower>(
                    choiceContext,
                    player.Creature,
                    1,
                    player.Creature,
                    null,
                    false);
            }
        }
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
        Player? player = StatusSlotManager.GetPlayerForCreature(target);
        if (player == null || !ReferenceEquals(player.Creature, target) ||
            !StatusSlotManager.IsSlotEnabled(player, StatusSlotType.Aberration))
        {
            return;
        }

        StatusSlotPlayerState state = StatusSlotManager.GetState(player);
        if (result.TotalDamage > 0)
            state.DamageEventsThisCombat++;
        if (result.UnblockedDamage > 0)
            state.DamageTakenThisCombat += (int)Math.Ceiling((decimal)result.UnblockedDamage);
        StatusSlotManager.CommitState(player, state);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);
        foreach (Player player in StatusSlotManager.GetOrderedPlayers())
        {
            await StatusSlotManager.DecrementAfterCombatAsync(player, StatusSlotType.Revelation);
            await StatusSlotManager.DecrementAfterCombatAsync(player, StatusSlotType.Aberration);

            if (Has(player, StatusSlotType.SwarmCall, "echo_long_road"))
            {
                StatusSlotPlayerState state = StatusSlotManager.GetState(player);
                state.EchoLongRoadCombatCount++;
                StatusSlotManager.CommitState(player, state);
                await CreatureCmd.GainMaxHp(player.Creature, 3);
            }
        }

        StatusSlotAberrationAction action = StatusSlotManager.BuildAberrationAction();
        await StatusSlotManager.ApplyAberrationActionAsync(action, new ThrowingPlayerChoiceContext());
        StatusSlotManager.RefreshUI();
    }

    public override async Task AfterModifyingGoldGained(Player player, decimal amount)
    {
        await base.AfterModifyingGoldGained(player, amount);
        if (amount <= 0)
            return;

        if (Has(player, StatusSlotType.SwarmCall, "echo_wither") &&
            EchoWitherBonusInProgress.Add(player.NetId))
        {
            try
            {
                await PlayerCmd.GainGold(amount, player);
            }
            finally
            {
                EchoWitherBonusInProgress.Remove(player.NetId);
            }
        }

        if (Has(player, StatusSlotType.SwarmCall, "echo_modification"))
        {
            StatusSlotPlayerState state = StatusSlotManager.GetState(player);
            state.EchoModificationGoldGained += (int)amount;
            StatusSlotManager.CommitState(player, state);
        }
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (room == null || !IsCombatRoom(room.RoomType))
            return false;

        bool modified = false;
        StatusSlotPlayerState state = StatusSlotManager.GetState(player);
        if (state.ColumbiaPendingReward)
        {
            rewards.Add(new CardReward(CardCreationOptions.ForRoom(player, RoomType.Boss), 3, player));
            state.ColumbiaPendingReward = false;
            state.Revision++;
            StatusSlotManager.CommitState(player, state);
            modified = true;
        }

        // Supply rewards are strictly owner-scoped. Other players do not roll or receive this reward.
        if (Has(player, StatusSlotType.SwarmCall, "echo_supply") &&
            player.PlayerRng.Rewards.NextInt(10000) < 3000)
        {
            CardCreationOptions options = new(
                new CardPoolModel[] { player.Character.CardPool },
                CardCreationSource.Other,
                CardRarityOddsType.RegularEncounter);
            rewards.Add(new CardReward(options, 3, player));
            modified = true;
        }

        return modified;
    }

    private static bool Has(Player player, StatusSlotType type, string key)
        => StatusSlotManager.HasEffect(player, type) && StatusSlotManager.GetEffectKey(player, type) == key;

    private static bool IsCombatRoom(RoomType roomType)
        => roomType is RoomType.Monster or RoomType.Elite or RoomType.Boss;

}
