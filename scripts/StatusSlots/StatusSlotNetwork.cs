namespace Arknights_Mizuki.Scripts.StatusSlots;

public sealed class StatusSlotRoomAction
{
    public int ActIndex { get; set; }
    public int RoomCount { get; set; }
    public int RoomType { get; set; }
    public List<StatusSlotRoomPlayerAction> Players { get; set; } = new();
}

public sealed class StatusSlotRoomPlayerAction
{
    public ulong PlayerNetId { get; set; }
    public bool ActChanged { get; set; }
    public string? NewRevelationKey { get; set; }
    public string NewSwarmCallKey { get; set; } = "";
}

public sealed class StatusSlotAberrationAction
{
    public int RoomCount { get; set; }
    public List<StatusSlotAberrationPlayerAction> Players { get; set; } = new();
}

public sealed class StatusSlotAberrationPlayerAction
{
    public ulong PlayerNetId { get; set; }
    public string EffectKey { get; set; } = "";
}
