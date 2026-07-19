using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
using Godot;

namespace Arknights_Mizuki.Scripts.Pools;

public class MzkCardPool : TypeListCardPoolModel
{
    // 卡池的ID。必须唯一防撞车。
    public override string Title => "MOLU_MZK";
    public override string EnergyColorName => "mizuki";

    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://Arknights_Mizuki/images/ui/energy2.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://Arknights_Mizuki/images/energy_mizuki_big.png";

    // 卡池的主题色。
    public override Color DeckEntryCardColor => new(0.5f, 0.5f, 1f);

    public override Color EnergyOutlineColor => new(0.5f, 0.5f, 1f);

#pragma warning disable CS0618 // RitsuLib 文档当前仍推荐该方法用于卡池 RGB 卡框着色。
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateRgbShaderMaterial(0.5f, 0.5f, 1f);
#pragma warning restore CS0618
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    // 如果你使用自定义卡框图片，重写CustomFrame方法并返回你的卡框图片。
    // public override Texture2D? CustomFrame(ModCardTemplate card)
    // {
    //     return card.Type switch
    //     {
    //         CardType.Attack => PreloadManager.Cache.GetAsset<Texture2D>("res://test/images/card_frame_attack.png"),
    //         CardType.Power => PreloadManager.Cache.GetAsset<Texture2D>("res://test/images/card_frame_power.png"),
    //         _ => PreloadManager.Cache.GetAsset<Texture2D>("res://test/images/card_frame_skill.png"),
    //     };
    // }

    // 卡池是否是无色。例如事件、状态等卡池就是无色的。
    public override bool IsColorless => false;
}
