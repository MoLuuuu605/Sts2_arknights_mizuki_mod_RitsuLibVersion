using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Arknights_Mizuki.Scripts.Pools;

/// <summary>
/// 状态栏位遗物的共享遗物池。
/// 将启示、排异反应、回响的效果遗物注册到这个独立池子，
/// 使它们在图鉴中有独立的分类，同时不影响水月角色的遗物池。
/// </summary>
[RegisterSharedRelicPool]
public class StatusSlotRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "mizuki";
}
