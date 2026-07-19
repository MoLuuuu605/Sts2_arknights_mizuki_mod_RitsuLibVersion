namespace Arknights_Mizuki.Scripts.StatusSlots;

/// <summary>
/// 效果定义。每个效果有唯一 key（用于本地化查找和触发分发）。
/// </summary>
public static class StatusSlotEffects
{
    public record EffectDef(
        string Key,
        StatusSlotType Slot,
        string IconPath,
        int DefaultCharges,
        bool IsRandomObtainable = true
    );

    private const string IconBase = "res://Arknights_Mizuki/images/status_slots";

    // ── 启示效果 ───────────────────────────────────
    public static readonly EffectDef UrsusRoar = new("ursus_roar", StatusSlotType.Revelation,
        $"{IconBase}/ursars_roar.png", 5);
    public static readonly EffectDef YanErosion = new("yan_erosion", StatusSlotType.Revelation,
        $"{IconBase}/yan_erosion.png", 5);
    public static readonly EffectDef ColumbiaInnovation = new("columbia_innovation", StatusSlotType.Revelation,
        $"{IconBase}/columbia_innovation.png", 1);
    public static readonly EffectDef SamiFortitude = new("sami_fortitude", StatusSlotType.Revelation,
        $"{IconBase}/sami_fortitude.png", 3);
    public static readonly EffectDef VictoriaGlory = new("victoria_glory", StatusSlotType.Revelation,
        $"{IconBase}/victoria_glory.png", 3);
    public static readonly EffectDef LeithanienElegance = new("leithanien_elegance", StatusSlotType.Revelation,
        $"{IconBase}/leithanien_elegance.png", 3);
    public static readonly EffectDef SargonGenerosity = new("sargon_generosity", StatusSlotType.Revelation,
        $"{IconBase}/sargon_generosity.png", 1);

    // ── 排异效果 ───────────────────────────────────
    public static readonly EffectDef HematopoieticDisorder = new("hematopoietic_disorder", StatusSlotType.Aberration,
        $"{IconBase}/hematopoietic_disorder.png", 5);
    public static readonly EffectDef FocusDisorder = new("focus_disorder", StatusSlotType.Aberration,
        $"{IconBase}/focus_disorder.png", 3);
    public static readonly EffectDef Neurodegeneration = new("neurodegeneration", StatusSlotType.Aberration,
        $"{IconBase}/neurodegeneration.png", 3);
    public static readonly EffectDef FleshDistortion = new("flesh_distortion", StatusSlotType.Aberration,
        $"{IconBase}/flesh_distortion.png", 5);

    // ── 回响效果 ───────────────────────────────────
    public static readonly EffectDef EchoStrife = new("echo_strife", StatusSlotType.SwarmCall,
        $"{IconBase}/echo_strife.png", 999);
    public static readonly EffectDef EchoModification = new("echo_modification", StatusSlotType.SwarmCall,
        $"{IconBase}/echo_modification.png", 999);
    public static readonly EffectDef EchoWither = new("echo_wither", StatusSlotType.SwarmCall,
        $"{IconBase}/echo_wither.png", 999);
    public static readonly EffectDef EchoSupply = new("echo_supply", StatusSlotType.SwarmCall,
        $"{IconBase}/echo_supply.png", 999);
    public static readonly EffectDef EchoLongRoad = new("echo_long_road", StatusSlotType.SwarmCall,
        $"{IconBase}/echo_long_road.png", 999);
    public static readonly EffectDef EchoAllMe = new("echo_allme", StatusSlotType.SwarmCall,
        $"{IconBase}/echo_allme.png", 999, IsRandomObtainable: false);

    public static readonly EffectDef[] All =
    {
        UrsusRoar, YanErosion, ColumbiaInnovation, SamiFortitude, VictoriaGlory,
        LeithanienElegance, SargonGenerosity,
        HematopoieticDisorder, FocusDisorder, Neurodegeneration, FleshDistortion,
        EchoStrife, EchoModification, EchoWither, EchoSupply, EchoLongRoad, EchoAllMe,
    };

    public static EffectDef? FindByKey(string key)
    {
        foreach (var e in All)
            if (e.Key == key) return e;
        return null;
    }

    public static System.Collections.Generic.List<EffectDef> GetForSlot(StatusSlotType slot)
    {
        var list = new System.Collections.Generic.List<EffectDef>();
        foreach (var e in All)
            if (e.Slot == slot) list.Add(e);
        return list;
    }
}
