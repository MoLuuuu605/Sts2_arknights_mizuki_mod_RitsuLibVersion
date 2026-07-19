using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using Arknights_Mizuki.Scripts.Powers;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 状态栏位数据存储 + 效果触发器。
/// 作为 ModifierModel 挂到 RunState 上，用 [SavedProperty] 自动存档/读档。
/// override AbstractModel 的战斗钩子方法来触发效果。
/// 顶栏图标通过 HideDataModifierBadge() 隐藏。
/// </summary>
public sealed class StatusSlotDataModifier : ModifierModel
{
    // 需要接收战斗钩子来触发效果
    public override bool ShouldReceiveCombatHooks => true;

    // ── 槽位数据 ───────────────────────────────────

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string RevelationKey { get; set; } = "";
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int RevelationCharges { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string AberrationKey { get; set; } = "";
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int AberrationCharges { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string SwarmCallKey { get; set; } = "";
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int SwarmCallCharges { get; set; }

    // ── 特殊状态追踪 ─────────────────────────────────

    /// <summary>哥伦比亚的创想：是否有待领取的稀有卡牌奖励</summary>
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ColumbiaPendingReward { get; set; }

    /// <summary>回响改造：本层获得的金币总量</summary>
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int EchoModificationGoldGained { get; set; }

    /// <summary>回响改造：获取时的金币量（用于清除时计算退还）</summary>
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int EchoModificationGoldOnAcquire { get; set; }

    /// <summary>进入新 Act 时用于检测 Act 变化</summary>
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int LastActIndex { get; set; } = -1;

    /// <summary>血肉畸变：失去的最大生命值</summary>
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int FleshDistortionLostHp { get; set; }

    /// <summary>回响途长：本层战斗次数（用于 +3 MaxHp）</summary>
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int EchoLongRoadCombatCount { get; set; }

    /// <summary>本 run 使用的状态槽设置快照。多人时固定为默认值，避免各客户端本地设置不同。</summary>
    [SavedProperty]
    public bool RunSettingsCaptured { get; set; }

    [SavedProperty]
    public bool RevelationEnabledForRun { get; set; } = StatusSlotSettings.DefaultRevelationEnabled;

    [SavedProperty]
    public bool AberrationEnabledForRun { get; set; } = StatusSlotSettings.DefaultAberrationEnabled;

    [SavedProperty]
    public bool SwarmCallEnabledForRun { get; set; } = StatusSlotSettings.DefaultSwarmCallEnabled;

    [SavedProperty]
    public bool RevelationAffectsOtherCharactersForRun { get; set; } = StatusSlotSettings.DefaultRevelationAffectsOtherCharacters;

    [SavedProperty]
    public bool AberrationAffectsOtherCharactersForRun { get; set; } = StatusSlotSettings.DefaultAberrationAffectsOtherCharacters;

    [SavedProperty]
    public bool SwarmCallAffectsOtherCharactersForRun { get; set; } = StatusSlotSettings.DefaultSwarmCallAffectsOtherCharacters;

    [SavedProperty]
    public int SwarmCallChanceForRun { get; set; } = ToChanceBasisPoints(StatusSlotSettings.DefaultSwarmCallChance);

    [SavedProperty]
    public int RevelationChanceForRun { get; set; } = ToChanceBasisPoints(StatusSlotSettings.DefaultRevelationChance);

    [SavedProperty]
    public int AberrationHpThresholdForRun { get; set; } = (int)StatusSlotSettings.DefaultAberrationHpThreshold;

    [SavedProperty]
    public int AberrationChanceForRun { get; set; } = ToChanceBasisPoints(StatusSlotSettings.DefaultAberrationChance);

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int DamageTakenThisCombat { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int DamageEventsThisCombat { get; set; }

    public void EnsureRunSettingsCaptured(IRunState runState)
    {
        if (RunSettingsCaptured)
            return;

        RevelationEnabledForRun = StatusSlotSettings.IsRevelationEnabled;
        AberrationEnabledForRun = StatusSlotSettings.IsAberrationEnabled;
        SwarmCallEnabledForRun = StatusSlotSettings.IsSwarmCallEnabled;
        RevelationAffectsOtherCharactersForRun = StatusSlotSettings.DoesRevelationAffectOtherCharacters;
        AberrationAffectsOtherCharactersForRun = StatusSlotSettings.DoesAberrationAffectOtherCharacters;
        SwarmCallAffectsOtherCharactersForRun = StatusSlotSettings.DoesSwarmCallAffectOtherCharacters;
        SwarmCallChanceForRun = ToChanceBasisPoints(StatusSlotSettings.SwarmCallChanceValue);
        RevelationChanceForRun = ToChanceBasisPoints(StatusSlotSettings.RevelationChanceValue);
        AberrationHpThresholdForRun = StatusSlotSettings.AberrationHpThresholdValue;
        AberrationChanceForRun = ToChanceBasisPoints(StatusSlotSettings.AberrationChanceValue);

        RunSettingsCaptured = true;
    }

    public bool IsMechanismEnabledForRun(StatusSlotType type) => type switch
    {
        StatusSlotType.Revelation => RevelationEnabledForRun,
        StatusSlotType.Aberration => AberrationEnabledForRun,
        StatusSlotType.SwarmCall => SwarmCallEnabledForRun,
        _ => false
    };

    public bool CanAffectOtherCharactersForRun(StatusSlotType type) => type switch
    {
        StatusSlotType.Revelation => RevelationAffectsOtherCharactersForRun,
        StatusSlotType.Aberration => AberrationAffectsOtherCharactersForRun,
        StatusSlotType.SwarmCall => SwarmCallAffectsOtherCharactersForRun,
        _ => false
    };

    public bool IsSlotEnabledForRun(StatusSlotType type)
        => IsMechanismEnabledForRun(type) && StatusSlotManager.GetStatusSlotOwnerPlayer(type) != null;

    // ── 便捷访问 ───────────────────────────────────

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
            case StatusSlotType.Revelation: RevelationKey = key; RevelationCharges = charges; break;
            case StatusSlotType.Aberration: AberrationKey = key; AberrationCharges = charges; break;
            case StatusSlotType.SwarmCall: SwarmCallKey = key; SwarmCallCharges = charges; break;
        }
    }

    public void ClearSlot(StatusSlotType type) => SetSlot(type, "", 0);
    public bool HasEffect(StatusSlotType type) => IsSlotEnabledForRun(type) && !string.IsNullOrEmpty(GetKey(type)) && GetCharges(type) > 0;
    private bool HasRevelation(string key) => HasEffect(StatusSlotType.Revelation) && RevelationKey == key;
    private bool HasAberration(string key) => HasEffect(StatusSlotType.Aberration) && AberrationKey == key;
    private bool HasSwarmCall(string key) => HasEffect(StatusSlotType.SwarmCall) && SwarmCallKey == key;

    private Player? GetPlayer(StatusSlotType type) => StatusSlotManager.GetStatusSlotOwnerPlayer(type);

    private Player? GetAnyStatusSlotOwner()
        => GetPlayer(StatusSlotType.Revelation)
           ?? GetPlayer(StatusSlotType.Aberration)
           ?? GetPlayer(StatusSlotType.SwarmCall);

    private static int ToChanceBasisPoints(double chance)
    {
        if (chance <= 0.0) return 0;
        if (chance >= 1.0) return 10000;

        return Math.Clamp((int)Math.Round(chance * 10000.0), 0, 10000);
    }

    private static bool RollChance(Rng rng, int chanceBasisPoints)
    {
        if (chanceBasisPoints <= 0) return false;
        if (chanceBasisPoints >= 10000) return true;

        return rng.NextInt(10000) < chanceBasisPoints;
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        await base.AfterRoomEntered(room);

        await RemoveDisabledEffectsAsync();
        await HandleActChange();
        await HandleRoomEnteredEffects(room);
        StatusSlotManager.RefreshUI();
    }

    private async Task RemoveDisabledEffectsAsync()
    {
        foreach (var slotType in new[] { StatusSlotType.Revelation, StatusSlotType.Aberration, StatusSlotType.SwarmCall })
        {
            if (IsSlotEnabledForRun(slotType))
                continue;

            string key = GetKey(slotType);
            if (string.IsNullOrEmpty(key) || GetCharges(slotType) <= 0)
                continue;

            Entry.Logger.Info($"[StatusSlot] {slotType} disabled for run; removing stored effect {key}");
            await StatusSlotManager.RemoveEffectAsync(slotType);
        }
    }

    private async Task HandleActChange()
    {
        var runState = RunState ?? GetAnyStatusSlotOwner()?.RunState;
        if (runState == null)
            return;

        int currentActIndex = runState.CurrentActIndex;
        if (currentActIndex == LastActIndex)
            return;

        Entry.Logger.Info($"[StatusSlot] Act changed: {LastActIndex} -> {currentActIndex}");

        var swarmPlayer = GetPlayer(StatusSlotType.SwarmCall);
        if (swarmPlayer != null &&
            HasEffect(StatusSlotType.SwarmCall) &&
            GetKey(StatusSlotType.SwarmCall) == "echo_modification")
        {
            int goldOnAcquire = EchoModificationGoldOnAcquire;
            int currentGold = swarmPlayer.Gold;
            if (currentGold > goldOnAcquire)
            {
                int goldToRemove = currentGold - goldOnAcquire;
                Entry.Logger.Info($"[StatusSlot] echo_modification: refunding {goldToRemove} gold (from {currentGold} back to {goldOnAcquire})");
                await PlayerCmd.LoseGold(goldToRemove, swarmPlayer, MegaCrit.Sts2.Core.Entities.Gold.GoldLossType.Spent);
            }
            else
            {
                Entry.Logger.Info($"[StatusSlot] echo_modification: no gold to refund (current {currentGold} <= acquired {goldOnAcquire})");
            }
        }

        if (IsSlotEnabledForRun(StatusSlotType.SwarmCall))
            await StatusSlotManager.RemoveEffectAsync(StatusSlotType.SwarmCall);

        Rng rng = runState.Rng.Niche;
        if (IsSlotEnabledForRun(StatusSlotType.SwarmCall) && RollChance(rng, SwarmCallChanceForRun))
        {
            var swarmEffects = StatusSlotEffects.GetForSlot(StatusSlotType.SwarmCall)
                .Where(e => e.IsRandomObtainable).ToList();
            if (swarmEffects.Count > 0)
            {
                var picked = rng.NextItem(swarmEffects)!;
                await StatusSlotManager.AssignEffectAsync(StatusSlotType.SwarmCall, picked.Key);
                Entry.Logger.Info($"[StatusSlot] Act {currentActIndex}: assigned SwarmCall {picked.Key}");
            }
        }

        if (!HasEffect(StatusSlotType.Revelation) &&
            IsSlotEnabledForRun(StatusSlotType.Revelation) &&
            RollChance(rng, RevelationChanceForRun))
        {
            var revEffects = StatusSlotEffects.GetForSlot(StatusSlotType.Revelation);
            if (revEffects.Count > 0)
            {
                var picked = rng.NextItem(revEffects)!;
                await StatusSlotManager.AssignEffectAsync(StatusSlotType.Revelation, picked.Key);
                Entry.Logger.Info($"[StatusSlot] Act {currentActIndex}: assigned Revelation {picked.Key}");
            }
        }

        LastActIndex = currentActIndex;
    }

    private async Task HandleRoomEnteredEffects(AbstractRoom room)
    {
        var player = GetPlayer(StatusSlotType.SwarmCall);
        if (player == null) return;

        if (HasEffect(StatusSlotType.SwarmCall) && GetKey(StatusSlotType.SwarmCall) == "echo_modification")
        {
            if (!IsCombatRoom(room))
            {
                Entry.Logger.Info("[StatusSlot] echo_modification: +25 gold for non-combat room");
                await PlayerCmd.GainGold(25m, player);
            }
        }

        if (HasEffect(StatusSlotType.SwarmCall) && GetKey(StatusSlotType.SwarmCall) == "echo_wither")
        {
            if (IsCombatRoom(room))
            {
                Entry.Logger.Info("[StatusSlot] echo_wither: -2 HP for entering combat room");
                var ctx = new ThrowingPlayerChoiceContext();
                await CreatureCmd.Damage(ctx, player.Creature, 2, ValueProp.Unblockable, null, null);
            }
        }
    }

    private static bool IsCombatRoom(AbstractRoom room)
    {
        return room.RoomType == RoomType.Monster ||
               room.RoomType == RoomType.Elite ||
               room.RoomType == RoomType.Boss;
    }

    // ── 战斗开始 ───────────────────────────────────

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
        await RemoveDisabledEffectsAsync();
        DamageTakenThisCombat = 0;
        DamageEventsThisCombat = 0;
        var ctx = new ThrowingPlayerChoiceContext();
        var revelationPlayer = GetPlayer(StatusSlotType.Revelation);
        var aberrationPlayer = GetPlayer(StatusSlotType.Aberration);
        var swarmPlayer = GetPlayer(StatusSlotType.SwarmCall);
        var combatState = (revelationPlayer ?? aberrationPlayer ?? swarmPlayer)?.Creature.CombatState;
        if (combatState == null) return;

        // 启示：乌萨斯的怒号 — 所有敌人 2 层虚弱
        if (revelationPlayer != null && HasRevelation("ursus_roar"))
        {
            Entry.Logger.Info("[StatusSlot] ursus_roar: 2 Weak to all enemies");
            foreach (var enemy in combatState.Enemies)
                if (enemy.IsAlive) await PowerCmd.Apply<WeakPower>(ctx, enemy, 2, revelationPlayer.Creature, null, false);
        }

        // 启示：莱塔尼亚的优雅 — 获得 8 层覆甲
        if (revelationPlayer != null && HasRevelation("leithanien_elegance"))
        {
            Entry.Logger.Info("[StatusSlot] leithanien_elegance: 8 Plating to player");
            await PowerCmd.Apply<PlatingPower>(ctx, revelationPlayer.Creature, 8, revelationPlayer.Creature, null, false);
        }
        // 排异：神经退行 — 第一回合获得晕眩
        if (aberrationPlayer != null && HasAberration("neurodegeneration"))
        {
            Entry.Logger.Info("[StatusSlot] neurodegeneration: Ringing to player");
            await PowerCmd.Apply<RingingPower>(ctx, aberrationPlayer.Creature, 1, aberrationPlayer.Creature, null, false);
        }

        // 回响：给养 — 所有敌人 4 层覆甲
        if (swarmPlayer != null && HasSwarmCall("echo_supply"))
        {
            Entry.Logger.Info("[StatusSlot] echo_supply: 4 Plating to all enemies");
            foreach (var enemy in combatState.Enemies)
                if (enemy.IsAlive) await PowerCmd.Apply<PlatingPower>(ctx, enemy, 4, swarmPlayer.Creature, null, false);
        }


        // 
        if (swarmPlayer != null && HasSwarmCall("echo_allme"))
        {
            Entry.Logger.Info("[StatusSlot] echo_allme: granting powers");
            await PowerCmd.Apply<EchoAllMeDamagePower>(ctx, swarmPlayer.Creature, 5, swarmPlayer.Creature, null, false);
            await PowerCmd.Apply<EchoAllMeEnergyPower>(ctx, swarmPlayer.Creature, 10, swarmPlayer.Creature, null, false);
        }
    }

    // ── 回合开始 ───────────────────────────────────

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;

        var ctx = new ThrowingPlayerChoiceContext();

        // 启示：大炎的沧桑 — 每回合 +1 费
        var revelationPlayer = GetPlayer(StatusSlotType.Revelation);
        if (revelationPlayer != null && HasRevelation("yan_erosion"))
        {
            Entry.Logger.Info("[StatusSlot] yan_erosion: +1 energy");
            await PlayerCmd.GainEnergy(1m, revelationPlayer);
        }

        // 回响：争斗 — 双方所有单位 +1 力量
        var swarmPlayer = GetPlayer(StatusSlotType.SwarmCall);
        if (swarmPlayer != null && HasSwarmCall("echo_strife"))
        {
            Entry.Logger.Info("[StatusSlot] echo_strife: +1 Strength to all");
            foreach (var c in combatState.Allies)
                if (c.IsAlive && StatusSlotManager.ShouldSlotAffectCreature(StatusSlotType.SwarmCall, c))
                    await PowerCmd.Apply<StrengthPower>(ctx, c, 1, swarmPlayer.Creature, null, false);
            foreach (var c in combatState.Enemies)
                if (c.IsAlive) await PowerCmd.Apply<StrengthPower>(ctx, c, 1, swarmPlayer.Creature, null, false);
        }
    }

    // ── 回合结束 ───────────────────────────────────

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side != CombatSide.Player) return;

        var player = GetPlayer(StatusSlotType.Aberration);
        if (player == null) return;

        // 排异：造血障碍 — 血量低于 80% 失去 2 HP
        if (HasAberration("hematopoietic_disorder"))
        {
            var maxHp = player.Creature.MaxHp;
            var curHp = player.Creature.CurrentHp;
            if (maxHp > 0 && (decimal)curHp < (decimal)maxHp * 0.8m)
            {
                Entry.Logger.Info($"[StatusSlot] hematopoietic_disorder: HP {curHp}/{maxHp} < 80%, lose 1 HP");
                var ctx = new ThrowingPlayerChoiceContext();
                await CreatureCmd.Damage(ctx, player.Creature, 1, ValueProp.Unblockable, null, null);
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

        if (target != GetPlayer(StatusSlotType.Aberration)?.Creature)
            return;

        if (result.TotalDamage > 0)
            DamageEventsThisCombat++;

        if (result.UnblockedDamage > 0)
            DamageTakenThisCombat += (int)Math.Ceiling((decimal)result.UnblockedDamage);
    }

    // ── 战斗结束 ───────────────────────────────────

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);

        // 战斗结束后递减次数
        await DecrementAfterCombat(StatusSlotType.Revelation);
        await DecrementAfterCombat(StatusSlotType.Aberration);

        // 回响：途长 — +3 MaxHp
        var swarmPlayer = GetPlayer(StatusSlotType.SwarmCall);
        if (swarmPlayer != null && HasSwarmCall("echo_long_road"))
        {
            EchoLongRoadCombatCount++;
            Entry.Logger.Info("[StatusSlot] echo_long_road: +3 MaxHp");
            await CreatureCmd.GainMaxHp(swarmPlayer.Creature, 3);
        }

        // ── 排异反应判定：单场受伤超阈值时概率获得 ──
        var aberrationPlayer = GetPlayer(StatusSlotType.Aberration);
        if (aberrationPlayer != null &&
            IsSlotEnabledForRun(StatusSlotType.Aberration) &&
            DamageTakenThisCombat >= AberrationHpThresholdForRun &&
            RollChance(aberrationPlayer.RunState.Rng.Niche, AberrationChanceForRun) &&
            !HasEffect(StatusSlotType.Aberration))
        {
            var aberEffects = StatusSlotEffects.GetForSlot(StatusSlotType.Aberration);
            if (aberEffects.Count > 0)
            {
                var picked = aberrationPlayer.RunState.Rng.Niche.NextItem(aberEffects)!;
                await StatusSlotManager.AssignEffectAsync(StatusSlotType.Aberration, picked.Key);
                Entry.Logger.Info($"[StatusSlot] DamageTaken={DamageTakenThisCombat} >= threshold={AberrationHpThresholdForRun}, assigned Aberration {picked.Key}");
            }
        }

        // 刷新顶栏 UI
        StatusSlotManager.RefreshUI();
    }

    public override async Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
    {
        if(!HasAberration("focus_disorder"))return;
        if (player != GetPlayer(StatusSlotType.Aberration)) return;
		ICombatState combatState = player.Creature.CombatState;
		if (player.PlayerCombatState.TurnNumber > 1)
		{
			return;
		}
		bool flag;
		using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
		{
			int cardsPlayed = 0;
			int startTurn = player.PlayerCombatState.TurnNumber;
			for (; cardsPlayed < 13; cardsPlayed++)
			{
				if (CombatManager.Instance.IsOverOrEnding)
				{
					break;
				}
				if (CombatManager.Instance.IsPlayerReadyToEndTurn(player))
				{
					break;
				}
				if (player.PlayerCombatState.TurnNumber != startTurn)
				{
					break;
				}
				CardPile pile = PileType.Hand.GetPile(player);
				CardModel card = pile.Cards.FirstOrDefault((CardModel c) => c.CanPlay());
				if (card == null)
				{
					break;
				}
				Creature target = GetTarget(card, combatState,player);
				await card.SpendResources();
				await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
			}
			flag = cardsPlayed >= 13;
			if (cardsPlayed == 0)
			{
				return;
			}
		}
    }

    // ── 伤害修改：加法 ───────────────────────────────

    public override decimal ModifyDamageAdditive(
        Creature target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        var result = base.ModifyDamageAdditive(target, amount, props, dealer, cardSource, cardPlay);

        // ???????? ? ???? -2???? 0?
        if (HasRevelation("sami_fortitude") && target == GetPlayer(StatusSlotType.Revelation)?.Creature)
        {
            result = Math.Max(0, result - 2);
        }

        return result;
    }

    // ── 伤害修改：乘法 ───────────────────────────────

    public override decimal ModifyDamageMultiplicative(
        Creature target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        var result = base.ModifyDamageMultiplicative(target, amount, props, dealer, cardSource, cardPlay);
        var revelationCreature = GetPlayer(StatusSlotType.Revelation)?.Creature;
        var swarmCreature = GetPlayer(StatusSlotType.SwarmCall)?.Creature;

        // 启示：维多利亚的荣耀 — 25% 概率取消受伤
        if (HasRevelation("victoria_glory") && target == revelationCreature)
        {
            if (ShouldCancelDamageWithVictoriaGlory(target, amount, props, dealer, cardSource, cardPlay))
            {
                return 0m;
            }
        }

        // 回响：途长 — 受到的伤害 +50%
        if (HasSwarmCall("echo_long_road") && target == swarmCreature)
        {
            result *= 1.5m;
        }

        return result;
    }

    private bool ShouldCancelDamageWithVictoriaGlory(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (amount <= 0)
            return false;

        return StableDamageRoll(target, amount, props, dealer, cardSource, cardPlay) < 25;
    }

    private int StableDamageRoll(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        var combatState = target.CombatState;

        unchecked
        {
            uint hash = 2166136261u;
            hash = HashInt(hash, RunState?.CurrentActIndex ?? 0);
            hash = HashInt(hash, RunState?.TotalFloor ?? 0);
            hash = HashInt(hash, combatState?.RoundNumber ?? 0);
            hash = HashInt(hash, GetAnyStatusSlotOwner()?.PlayerCombatState.TurnNumber ?? 0);
            hash = HashInt(hash, DamageEventsThisCombat);
            hash = HashInt(hash, GetCreatureIndex(target, combatState));
            hash = HashInt(hash, GetCreatureIndex(dealer, combatState));
            hash = HashInt(hash, (int)amount);
            hash = HashInt(hash, (int)props);
            hash = HashInt(hash, cardPlay?.PlayIndex ?? -1);
            hash = HashString(hash, cardSource?.GetType().FullName ?? "");
            return (int)(hash % 100u);
        }
    }

    private static uint HashInt(uint hash, int value)
    {
        unchecked
        {
            hash ^= (uint)value;
            return hash * 16777619u;
        }
    }

    private static uint HashString(uint hash, string value)
    {
        unchecked
        {
            foreach (char c in value)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return hash;
        }
    }

    private static int GetCreatureIndex(Creature? creature, ICombatState? combatState)
    {
        if (creature == null || combatState == null)
            return -1;

        int allyIndex = IndexOf(combatState.Allies, creature);
        if (allyIndex >= 0)
            return allyIndex;

        int enemyIndex = IndexOf(combatState.Enemies, creature);
        if (enemyIndex >= 0)
            return 1000 + enemyIndex;

        return -1;
    }

    private static int IndexOf(IReadOnlyList<Creature> creatures, Creature creature)
    {
        for (int i = 0; i < creatures.Count; i++)
        {
            if (ReferenceEquals(creatures[i], creature))
                return i;
        }

        return -1;
    }

    // ── 金币修改 ───────────────────────────────────

    public override async Task AfterModifyingGoldGained(Player player, decimal amount)
    {
        await base.AfterModifyingGoldGained(player, amount);
        if (player != GetPlayer(StatusSlotType.SwarmCall))
            return;

        // 回响：枯荣 — 战斗掉落金币 +100%
        if (HasSwarmCall("echo_wither") && amount > 0)
        {
            Entry.Logger.Info($"[StatusSlot] echo_wither: gold +100% = {amount}");
            await PlayerCmd.GainGold(amount, player);
        }

        // 回响：改造 — 追踪本层获得的金币
        if (HasSwarmCall("echo_modification") && amount > 0)
        {
            EchoModificationGoldGained += (int)amount;
        }
    }

    // ── 战斗奖励 ───────────────────────────────────

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (room == null || !IsCombatRoom(room))
            return false;

        bool modified = false;

        if (player == GetPlayer(StatusSlotType.Revelation) && HasRevelation("columbia_innovation"))
        {
            Entry.Logger.Info("[StatusSlot] columbia_innovation: 1 extra rare card reward");
            rewards.Add(new CardReward(CardCreationOptions.ForRoom(player, RoomType.Boss), 3, player));
            modified = true;
        }

        if (player == GetPlayer(StatusSlotType.SwarmCall) &&
            HasSwarmCall("echo_supply") &&
            RollChance(player.PlayerRng.Rewards, 3000))
        {
            Entry.Logger.Info("[StatusSlot] echo_supply: 30% extra card reward");
            CardCreationOptions options = new(
                new CardPoolModel[] { player.Character.CardPool },
                CardCreationSource.Other,
                CardRarityOddsType.RegularEncounter);
            rewards.Add(new CardReward(options, 3, player));
            modified = true;
        }

        return modified;
    }

    public override async Task BeforeCombatRewardOffered(RewardsSet rewardsSet, CombatRoom room)
    {
        await base.BeforeCombatRewardOffered(rewardsSet, room);

        var player = GetPlayer(StatusSlotType.Revelation);
        if (player == null) return;

        // 启示：哥伦比亚的创想 — 额外稀有卡牌奖励
        if (ColumbiaPendingReward)
        {
            ColumbiaPendingReward = false;
            Entry.Logger.Info("[StatusSlot] columbia_innovation: adding rare card reward");
            // TODO: 添加稀有卡牌奖励到 rewardsSet
            // rewardsSet.AddReward(new CardReward(..., rarity: Rarity.Rare));
        }
    }

    // ── 次数递减 ───────────────────────────────────

    /// <summary>
    /// 战斗结束后递减次数。归零时清除槽位并触发失去效果。
    /// </summary>
    private async Task DecrementAfterCombat(StatusSlotType slotType)
    {
        var charges = GetCharges(slotType);
        if (charges <= 0) return;
        var key = GetKey(slotType);
        if (string.IsNullOrEmpty(key)) return;

        charges--;
        Entry.Logger.Info($"[StatusSlot] {slotType} charges after combat: {charges}");

        if (charges <= 0)
        {
            Entry.Logger.Info($"[StatusSlot] {slotType} expired, slot cleared");
            await StatusSlotManager.RemoveEffectAsync(slotType);
        }
        else
        {
            SetSlot(slotType, key, charges);
        }
    }
    	private Creature? GetTarget(CardModel card, ICombatState combatState,Player player)
	{
		Rng combatTargets = player.RunState.Rng.CombatTargets;
		return card.TargetType switch
		{
			TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(), 
			TargetType.AnyAlly => combatTargets.NextItem(combatState.Allies.Where((Creature c) => c != null && c.IsAlive && c.IsPlayer && c != player.Creature)), 
			TargetType.AnyPlayer => player.Creature, 
			_ => null, 
		};
	}
}
