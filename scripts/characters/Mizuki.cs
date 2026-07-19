using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Godot;
using Arknights_Mizuki.Scripts.Cards;
using Arknights_Mizuki.Scripts.Utils;
using Arknights_Mizuki.Scripts.Relics;
using Arknights_Mizuki.Scripts.Pools;
using STS2RitsuLib.Scaffolding.Characters;

namespace Arknights_Mizuki.Scripts.Characters;


[RegisterCharacter]
public class Mizuki : ModCharacterTemplate<MzkCardPool, MzkRelicPool, MzkPotionPool>
{

    public override Color NameColor => new(0.3f, 0.5f, 1f);

    public override Color EnergyLabelOutlineColor => new(0.1f, 0.2f, 0.7f);

    public override Color MapDrawingColor => new(0.3f, 0.5f, 1f);

    public override CharacterGender Gender => CharacterGender.Masculine;

    public override int StartingHp => 70;

    public override int StartingGold => 99;

    public override float AttackAnimDelay => 0f;

    public override float CastAnimDelay => 0f;

    public override string CustomVisualsPath => "res://Arknights_Mizuki/scenes/character.tscn";

    public override string CustomIconTexturePath => "res://Arknights_Mizuki/images/icon.svg";

    public override CreatureAnimator GenerateAnimator(MegaSprite controller) => SpineAnimatorFactory.Create(controller, cast: "Skill_1", buff: "Skill_1", summon: "Skill_1");

    public override string CustomEnergyCounterPath => "res://Arknights_Mizuki/scenes/energy_counter.tscn";

    public override string CustomCharacterSelectBgPath => "res://Arknights_Mizuki/scenes/bg.tscn";

    public override string CustomCharacterSelectIconPath => "res://Arknights_Mizuki/images/icon.png";

    public override string CustomCharacterSelectLockedIconPath => "res://Arknights_Mizuki/images/icon.png";

    public override string CustomRestSiteAnimPath => "res://Arknights_Mizuki/scenes/rest_site.tscn";

    public override string CustomMerchantAnimPath => "res://Arknights_Mizuki/scenes/test_character_merchant.tscn";

    public override string CharacterSelectSfx => "res://Arknights_Mizuki/audios/select.wav";

    public override string CharacterTransitionSfx => "res://Arknights_Mizuki/audios/pass.wav";

    public override string CustomIconPath => "res://Arknights_Mizuki/scenes/iconpath.tscn";

    public override string CustomArmPointingTexturePath =>"res://Arknights_Mizuki/images/ui/point.png";
    public override string CustomArmRockTexturePath => "res://Arknights_Mizuki/images/ui/rock.png";
    public override string CustomArmPaperTexturePath => "res://Arknights_Mizuki/images/ui/paper.png";
    public override string CustomArmScissorsTexturePath => "res://Arknights_Mizuki/images/ui/scissors.png";

    // 初始卡组
    [Obsolete]
    protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
        new(typeof(MzkStrike), 5),
        new(typeof(MzkDefence), 4),
        new(typeof(Awaken), 1)
    ];

    // 初始遗物（暂时空，等确认游戏内遗物类名后替换）
    [Obsolete]
    protected override IEnumerable<Type> StartingRelicTypes => [
        typeof(MzkTreeBranch),
        typeof(Relics.Key)
    ];

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];
}
