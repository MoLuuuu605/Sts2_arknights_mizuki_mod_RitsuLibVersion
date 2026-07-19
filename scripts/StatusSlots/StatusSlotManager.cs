using Arknights_Mizuki.Scripts.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 状态栏位管理器。负责数据存取、效果添加/移除、UI 刷新。
/// 数据存在 StatusSlotDataModifier（挂到 RunState 上的 ModifierModel）里，
/// 通过 [SavedProperty] 自动存档/读档，自动支持多人同步。
/// </summary>
public static class StatusSlotManager
{
    private static StatusSlotDataModifier? _data;
    private static RunState? _runState;

    // ── 数据存取 ───────────────────────────────────

    /// <summary>
    /// 确保数据已加载。只查找或创建 StatusSlotDataModifier，不挂到 RunState.Modifiers。
    /// 新局开始时调用——避免在 Neow 选项生成前让 Modifiers.Count > 0，否则 Neow 会跳过正常选项。
    /// </summary>
    public static void EnsureData(RunState runState)
    {
        _runState = runState;
        _data = GetDataModifier(runState);
        _data.EnsureRunSettingsCaptured(runState);
        Entry.Logger.Info($"[StatusSlot] Data ensured. Rev={_data.RevelationKey}/{_data.RevelationCharges}, Aber={_data.AberrationKey}/{_data.AberrationCharges}, Swarm={_data.SwarmCallKey}/{_data.SwarmCallCharges}");
    }

    /// <summary>
    /// 将 StatusSlotDataModifier 挂到 RunState.Modifiers，使其被存档系统序列化。
    /// 应在 Neow 事件结束后（进入非 Ancient 房间时）调用，避免影响 Neow 选项生成。
    /// </summary>
    public static void AttachModifier(RunState runState)
    {
        if (_data == null) return;

        // 已挂载则跳过
        if (runState.Modifiers.OfType<StatusSlotDataModifier>().Any())
        {
            _data = runState.Modifiers.OfType<StatusSlotDataModifier>().Last();
            _data.EnsureRunSettingsCaptured(runState);
            return;
        }

        // 新局首次：数据是静态的，尚未挂载；挂到 Modifiers 让存档系统接管
        runState.AddModifierDebug(_data);
        Entry.Logger.Info("[StatusSlot] Attached StatusSlotDataModifier to RunState.Modifiers");
    }

    public static StatusSlotDataModifier? GetData() => _data;
    public static RunState? GetRunState() => _runState;

    internal static Player? GetPrimaryPlayer()
    {
        var runState = _runState;
        if (runState == null || runState.Players.Count == 0)
            return null;

        return GetOrderedPlayers(runState).FirstOrDefault();
    }

    internal static Player? GetStatusSlotOwnerPlayer(StatusSlotType type)
    {
        var runState = _runState;
        if (runState == null || runState.Players.Count == 0)
            return null;

        if (!IsMechanismEnabled(type))
            return null;

        var orderedPlayers = GetOrderedPlayers(runState).ToList();
        var mizuki = orderedPlayers.FirstOrDefault(IsMizukiPlayer);
        if (mizuki != null)
            return mizuki;

        return CanAffectOtherCharacters(type)
            ? orderedPlayers.FirstOrDefault()
            : null;
    }

    internal static bool IsPlayerEligibleForSlot(Player player, StatusSlotType type)
    {
        if (!IsMechanismEnabled(type))
            return false;

        return IsMizukiPlayer(player) || CanAffectOtherCharacters(type);
    }

    internal static bool ShouldSlotAffectCreature(StatusSlotType type, Creature creature)
    {
        var owner = GetPlayerForCreature(creature);
        return owner == null || IsPlayerEligibleForSlot(owner, type);
    }

    internal static bool IsMizukiPlayer(Player player) => player.Character is Mizuki;

    private static IEnumerable<Player> GetOrderedPlayers(RunState runState)
    {
        return runState.Players
            .Select(p => new { Player = p, SlotIndex = runState.GetPlayerSlotIndex(p) })
            .OrderBy(x => x.SlotIndex < 0 ? int.MaxValue : x.SlotIndex)
            .Select(x => x.Player);
    }

    private static Player? GetPlayerForCreature(Creature creature)
    {
        var runState = _runState;
        if (runState == null)
            return null;

        foreach (var player in runState.Players)
        {
            if (ReferenceEquals(player.Creature, creature) ||
                ReferenceEquals(creature.PetOwner?.Creature, player.Creature))
            {
                return player;
            }
        }

        return null;
    }

    internal static bool IsSlotEnabled(StatusSlotType type)
    {
        if (_data?.RunSettingsCaptured == true)
            return _data.IsSlotEnabledForRun(type);

        if (_runState != null)
            return GetStatusSlotOwnerPlayer(type) != null;

        return IsMechanismEnabled(type);
    }

    internal static bool IsSlotVisibleForLocalPlayer(StatusSlotType type)
    {
        if (!IsSlotEnabled(type))
            return false;

        var runState = _runState;
        if (runState == null || runState.Players.Count == 0)
            return IsMechanismEnabled(type);

        var localPlayer = GetLocalPlayer(runState);
        return localPlayer != null && IsPlayerEligibleForSlot(localPlayer, type);
    }

    private static Player? GetLocalPlayer(RunState runState)
    {
        var localPlayer = LocalContext.GetMe(runState);
        if (localPlayer != null)
            return localPlayer;

        return runState.Players.Count == 1
            ? GetOrderedPlayers(runState).FirstOrDefault()
            : null;
    }

    private static bool IsMechanismEnabled(StatusSlotType type)
    {
        if (_data?.RunSettingsCaptured == true)
            return _data.IsMechanismEnabledForRun(type);

        return type switch
        {
            StatusSlotType.Revelation => StatusSlotSettings.IsRevelationEnabled,
            StatusSlotType.Aberration => StatusSlotSettings.IsAberrationEnabled,
            StatusSlotType.SwarmCall => StatusSlotSettings.IsSwarmCallEnabled,
            _ => false
        };
    }

    private static bool CanAffectOtherCharacters(StatusSlotType type)
    {
        if (_data?.RunSettingsCaptured == true)
            return _data.CanAffectOtherCharactersForRun(type);

        return type switch
        {
            StatusSlotType.Revelation => StatusSlotSettings.DoesRevelationAffectOtherCharacters,
            StatusSlotType.Aberration => StatusSlotSettings.DoesAberrationAffectOtherCharacters,
            StatusSlotType.SwarmCall => StatusSlotSettings.DoesSwarmCallAffectOtherCharacters,
            _ => false
        };
    }

    private static StatusSlotDataModifier GetDataModifier(RunState runState)
    {
        // 1. 优先从已挂载的 Modifiers 列表查找（读档场景）
        var existing = runState.Modifiers.OfType<StatusSlotDataModifier>().LastOrDefault();
        if (existing != null) return existing;

        // 2. 新局：创建但不挂载（避免影响 Neow），只调用 OnRunLoaded 设置 _runState
        var mod = (StatusSlotDataModifier)ModelDb.Modifier<StatusSlotDataModifier>().ToMutable();
        mod.OnRunLoaded(runState);

        Entry.Logger.Info("[StatusSlot] Created new StatusSlotDataModifier (not attached yet)");
        return mod;
    }

    // ── 效果管理 ───────────────────────────────────

    public static void AssignEffect(StatusSlotType slotType, string effectKey)
        => AssignEffectAsync(slotType, effectKey).GetAwaiter().GetResult();

    public static async Task AssignEffectAsync(StatusSlotType slotType, string effectKey)
    {
        if (_data == null)
        {
            Entry.Logger.Error("[StatusSlot] Cannot assign effect: data not initialized");
            return;
        }

        var def = StatusSlotEffects.FindByKey(effectKey);
        if (def == null || def.Slot != slotType)
        {
            Entry.Logger.Error($"[StatusSlot] Invalid effect '{effectKey}' for slot {slotType}");
            return;
        }

        if (!_data.IsSlotEnabledForRun(slotType))
        {
            Entry.Logger.Info($"[StatusSlot] Ignored {effectKey}: {slotType} is disabled for this run");
            _data.ClearSlot(slotType);
            RefreshUI();
            return;
        }

        _data.SetSlot(slotType, effectKey, def.DefaultCharges);
        Entry.Logger.Info($"[StatusSlot] Assigned {effectKey} to {slotType}, charges={def.DefaultCharges}");

        // ── 获得时的特殊效果 ─────────────────────────
        await HandleOnAcquiredAsync(slotType, effectKey);
        RefreshUI();
    }

    public static void RemoveEffect(StatusSlotType slotType)
        => RemoveEffectAsync(slotType).GetAwaiter().GetResult();

    public static async Task RemoveEffectAsync(StatusSlotType slotType)
    {
        if (_data == null) return;
        var key = _data.GetKey(slotType);
        _data.ClearSlot(slotType);
        Entry.Logger.Info($"[StatusSlot] Removed effect from {slotType}");

        // ── 失去时的特殊效果 ─────────────────────────
        if (!string.IsNullOrEmpty(key))
            await HandleOnRemovedAsync(slotType, key);
        RefreshUI();
    }

    public static bool HasEffect(StatusSlotType slotType)
        => _data != null && _data.HasEffect(slotType);

    public static string GetEffectKey(StatusSlotType type)
        => _data?.GetKey(type) ?? "";

    public static int GetCharges(StatusSlotType type)
        => _data?.GetCharges(type) ?? 0;

    public static void DecrementCharges(StatusSlotType slotType)
        => DecrementChargesAsync(slotType).GetAwaiter().GetResult();

    public static async Task DecrementChargesAsync(StatusSlotType slotType)
    {
        if (_data == null) return;
        if (!_data.IsSlotEnabledForRun(slotType))
        {
            _data.ClearSlot(slotType);
            RefreshUI();
            return;
        }

        var charges = _data.GetCharges(slotType);
        if (charges <= 0) return;

        charges--;
        var key = _data.GetKey(slotType);
        _data.SetSlot(slotType, key, charges);

        Entry.Logger.Info($"[StatusSlot] {slotType} charges decremented to {charges}");

        if (charges <= 0)
        {
            _data.ClearSlot(slotType);
            Entry.Logger.Info($"[StatusSlot] {slotType} effect used up, cleared");
            await HandleOnRemovedAsync(slotType, key);
        }
        RefreshUI();
    }

    // ── UI 刷新 ────────────────────────────────────

    public static void RefreshUI()
    {
        if (_data == null) return;

        for (int i = 0; i < 3; i++)
        {
            var slotType = (StatusSlotType)i;
            var key = _data.GetKey(slotType);
            var charges = _data.GetCharges(slotType);
            var enabled = _data.IsSlotEnabledForRun(slotType);
            var visible = IsSlotVisibleForLocalPlayer(slotType);
            StatusSlotFrame.SetSlotVisible(i, visible);

            if (!visible)
            {
                StatusSlotFrame.SetContent(i, (string?)null,
                    StatusSlotI18n.GetSlotName(slotType),
                    StatusSlotI18n.GetSlotEmpty(slotType));
            }
            else if (!enabled)
            {
                StatusSlotFrame.SetContent(i, (string?)null,
                    StatusSlotI18n.GetSlotName(slotType),
                    StatusSlotI18n.GetSlotEmpty(slotType));
            }
            else if (!string.IsNullOrEmpty(key) && charges > 0)
            {
                var def = StatusSlotEffects.FindByKey(key);
                if (def != null)
                {
                    string name = StatusSlotI18n.GetEffectName(key);
                    string desc = StatusSlotI18n.GetEffectDesc(key);
                    if (slotType != StatusSlotType.SwarmCall && charges < 999)
                    {
                        if (charges == 1)
                            desc += " \n（在本场战斗后清除）";
                        else
                            desc += $" \n（{charges} 场战斗后移除）";
                    }
                    StatusSlotFrame.SetContent(i, def.IconPath, name, desc);
                }
            }
            else
            {
                string emptyDesc;
                if (slotType == StatusSlotType.Aberration)
                {
                    int hp = _data.AberrationHpThresholdForRun;
                    int pct = (int)Math.Round(_data.AberrationChanceForRun / 100.0);
                    emptyDesc = $"单场战斗失去 {hp} 点以上生命值时，有 {pct}% 概率获得一个随机的排异反应。";
                }
                else
                {
                    emptyDesc = StatusSlotI18n.GetSlotEmpty(slotType);
                }
                StatusSlotFrame.SetContent(i, (string?)null,
                    StatusSlotI18n.GetSlotName(slotType),
                    emptyDesc);
            }
        }
    }

    // ── 获得/失去时的特殊效果 ────────────────────────

    private static async Task HandleOnAcquiredAsync(StatusSlotType slotType, string effectKey)
    {
        var player = GetStatusSlotOwnerPlayer(slotType);
        if (player == null) return;

        switch (effectKey)
        {
            case "sargon_generosity":
                // 萨尔贡的慷慨：随机获得 2 个遗物
                Entry.Logger.Info("[StatusSlot] sargon_generosity: granting 2 random relics");
                RelicModel relic = RelicFactory.PullNextRelicFromFront(player!).ToMutable();
                await RelicCmd.Obtain(relic, player!);
                RelicModel relic2 = RelicFactory.PullNextRelicFromFront(player!).ToMutable();
                await RelicCmd.Obtain(relic2, player!);
                break;

            case "flesh_distortion":
                // 血肉畸变：失去 50% 最大生命
                var maxHp = player.Creature.MaxHp;
                var lost = maxHp / 2;
                _data!.FleshDistortionLostHp = lost;
                Entry.Logger.Info($"[StatusSlot] flesh_distortion: lose {lost} MaxHp (was {maxHp})");
                await CreatureCmd.LoseMaxHp(
                    new ThrowingPlayerChoiceContext(),
                    player.Creature, lost, isFromCard: false);
                break;

            case "echo_modification":
                // 记录获取时的金币量
                _data!.EchoModificationGoldOnAcquire = player.Gold;
                _data!.EchoModificationGoldGained = 0;
                Entry.Logger.Info($"[StatusSlot] echo_modification: recorded GoldOnAcquire={player.Gold}");
                break;

            case "columbia_innovation":
                // 哥伦比亚的创想：标记下次战斗给稀有卡牌
                _data!.ColumbiaPendingReward = true;
                Entry.Logger.Info("[StatusSlot] columbia_innovation: pending rare card reward");
                break;
        }
    }

    private static async Task HandleOnRemovedAsync(StatusSlotType slotType, string effectKey)
    {
        var player = GetStatusSlotOwnerPlayer(slotType);
        if (player == null) return;

        switch (effectKey)
        {
            case "flesh_distortion":
                // 血肉畸变：恢复失去的最大生命
                var lost = _data?.FleshDistortionLostHp ?? 0;
                if (lost > 0)
                {
                    Entry.Logger.Info($"[StatusSlot] flesh_distortion: restore {lost} MaxHp");
                    await CreatureCmd.GainMaxHp(player.Creature, lost);
                    if (_data != null) _data.FleshDistortionLostHp = 0;
                }
                break;

            case "echo_modification":
                break;
        }
    }
}
