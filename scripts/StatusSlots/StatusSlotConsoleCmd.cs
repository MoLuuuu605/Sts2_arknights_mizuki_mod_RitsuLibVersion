using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 控制台命令：statusslot
/// 用法：
///   statusslot add revelation [effect_key]  — 添加启示效果（不指定 key 则用第一个）
///   statusslot add aberration [effect_key]  — 添加排异效果
///   statusslot add swarmcall [effect_key]   — 添加回响效果
///   statusslot remove revelation             — 移除启示效果
///   statusslot clear                         — 清空所有槽位
///   statusslot info                          — 查看当前状态
///   statusslot list [slot]                   — 列出槽位可用效果
///
/// 示例：
///   statusslot add revelation ursus_roar
///   statusslot list revelation
/// </summary>
public sealed class StatusSlotConsoleCmd : AbstractConsoleCmd
{
    private const string UsageText = "statusslot <add|remove|clear|info|list> [revelation|aberration|swarmcall] [effect_key]";

    public override string CmdName => "statusslot";

    public override string Args => "<add|remove|clear|info|list> [slot] [effect_key]";

    public override string Description => "管理状态栏位效果";

    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (!(issuingPlayer?.RunState is RunState runState) || !RunManager.Instance.IsInProgress)
        {
            return new CmdResult(success: false, "该命令只能在跑局中使用。");
        }

        if (args.Length == 0)
        {
            return new CmdResult(success: false, UsageText);
        }

        StatusSlotManager.EnsureData(runState);

        var action = args[0].ToLowerInvariant();

        return action switch
        {
            "add" => HandleAdd(args),
            "remove" => HandleRemove(args),
            "clear" => HandleClear(),
            "info" => HandleInfo(),
            "list" => HandleList(args),
            _ => new CmdResult(success: false, $"未知操作: {action}\n{UsageText}")
        };
    }

    private CmdResult HandleAdd(string[] args)
    {
        if (args.Length < 2)
        {
            return new CmdResult(success: false, "用法: statusslot add <revelation|aberration|swarmcall> [effect_key]");
        }

        var slot = ParseSlot(args[1]);
        if (slot == null)
        {
            return new CmdResult(success: false, $"未知槽位: {args[1]}\n可选: revelation, aberration, swarmcall");
        }

        // 如果指定了 effect_key，用指定的；否则用该槽位第一个效果
        string effectKey;
        if (!StatusSlotManager.IsSlotEnabled(slot.Value))
        {
            return new CmdResult(success: false, $"{slot.Value} is disabled for this run.");
        }

        if (args.Length >= 3)
        {
            effectKey = args[2];
            var def = StatusSlotEffects.FindByKey(effectKey);
            if (def == null || def.Slot != slot.Value)
            {
                return new CmdResult(success: false, $"效果 '{effectKey}' 不存在或不属于 {slot.Value} 槽位。\n用 'statusslot list {args[1]}' 查看可用效果。");
            }
        }
        else
        {
            var effects = StatusSlotEffects.GetForSlot(slot.Value);
            if (effects.Count == 0)
            {
                return new CmdResult(success: false, $"{slot.Value} 槽位没有可用效果。");
            }
            effectKey = effects[0].Key;
        }

        StatusSlotManager.AssignEffect(slot.Value, effectKey);

        var name = StatusSlotI18n.GetEffectName(effectKey);
        var charges = StatusSlotEffects.FindByKey(effectKey)!.DefaultCharges;
        return new CmdResult(success: true, $"已添加 {name} ({effectKey}) 到 {slot.Value}，剩余 {charges} 场。");
    }

    private CmdResult HandleRemove(string[] args)
    {
        if (args.Length < 2)
        {
            return new CmdResult(success: false, "用法: statusslot remove <revelation|aberration|swarmcall>");
        }

        var slot = ParseSlot(args[1]);
        if (slot == null)
        {
            return new CmdResult(success: false, $"未知槽位: {args[1]}");
        }

        StatusSlotManager.RemoveEffect(slot.Value);
        return new CmdResult(success: true, $"已移除 {slot.Value} 槽位的效果。");
    }

    private CmdResult HandleClear()
    {
        StatusSlotManager.RemoveEffect(StatusSlotType.Revelation);
        StatusSlotManager.RemoveEffect(StatusSlotType.Aberration);
        StatusSlotManager.RemoveEffect(StatusSlotType.SwarmCall);
        return new CmdResult(success: true, "已清空所有槽位。");
    }

    private CmdResult HandleInfo()
    {
        var data = StatusSlotManager.GetData();
        if (data == null)
        {
            return new CmdResult(success: false, "数据未初始化。");
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("状态栏位:");
        for (int i = 0; i < 3; i++)
        {
            var slotType = (StatusSlotType)i;
            var key = data.GetKey(slotType);
            var charges = data.GetCharges(slotType);
            if (!string.IsNullOrEmpty(key) && charges > 0)
            {
                var name = StatusSlotI18n.GetEffectName(key);
                sb.AppendLine($"  {slotType}: {name} ({key}), 剩余 {charges} 场");
            }
            else
            {
                sb.AppendLine($"  {slotType}: 空");
            }
        }
        return new CmdResult(success: true, sb.ToString().TrimEnd());
    }

    private CmdResult HandleList(string[] args)
    {
        if (args.Length < 2)
        {
            // 列出所有槽位的所有效果
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("所有可用效果:");
            foreach (var def in StatusSlotEffects.All)
            {
                var name = StatusSlotI18n.GetEffectName(def.Key);
                sb.AppendLine($"  [{def.Slot}] {def.Key} = {name} (默认 {def.DefaultCharges} 场)");
            }
            return new CmdResult(success: true, sb.ToString().TrimEnd());
        }

        var slot = ParseSlot(args[1]);
        if (slot == null)
        {
            return new CmdResult(success: false, $"未知槽位: {args[1]}");
        }

        var effects = StatusSlotEffects.GetForSlot(slot.Value);
        if (effects.Count == 0)
        {
            return new CmdResult(success: true, $"{slot.Value} 槽位没有可用效果。");
        }

        var sb2 = new System.Text.StringBuilder();
        sb2.AppendLine($"{slot.Value} 槽位可用效果:");
        foreach (var def in effects)
        {
            var name = StatusSlotI18n.GetEffectName(def.Key);
            sb2.AppendLine($"  {def.Key} = {name} (默认 {def.DefaultCharges} 场)");
        }
        return new CmdResult(success: true, sb2.ToString().TrimEnd());
    }

    private static StatusSlotType? ParseSlot(string s)
    {
        return s.ToLowerInvariant() switch
        {
            "revelation" or "rev" => StatusSlotType.Revelation,
            "aberration" or "aber" => StatusSlotType.Aberration,
            "swarmcall" or "swarm" or "echo" => StatusSlotType.SwarmCall,
            _ => null
        };
    }
}
