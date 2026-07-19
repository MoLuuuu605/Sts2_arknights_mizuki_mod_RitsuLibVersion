using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Arknights_Mizuki.Scripts.Acts;

namespace Arknights_Mizuki.Scripts.Enemies;

[RegisterActEncounter(typeof(EvolutionSingularityAct))]
public sealed class SingularityBossEncounter : ModEncounterTemplate
{
    private const string IconPath = "res://Arknights_Mizuki/images/ui/run_history/izumik_boss_encounter_icon";

    public override RoomType RoomType => RoomType.Boss;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
    {
        ModelDb.Monster<Izumik>(),
    };

    public override string BossNodePath => IconPath;

    public override string? CustomRunHistoryIconPath => IconPath + ".png";

    public override string? CustomRunHistoryIconOutlinePath => IconPath + "_outline.png";

    public override MegaSkeletonDataResource? BossNodeSpineResource => null;

    public override bool IsValidForAct(ActModel act)
    {
        return act is EvolutionSingularityAct;
    }

    // 复用 Glory 的战斗背景，避免游戏找 evolution_singularity_act/layers 目录
    protected override bool UseProgrammaticCombatBackground => true;

    protected override BackgroundAssets BuildProgrammaticCombatBackground(ActModel act, Rng rng)
        => ModelDb.Act<Glory>().GenerateBackgroundAssets(rng);

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new (MonsterModel, string?)[]
        {
            (ModelDb.Monster<Izumik>().ToMutable(), null)
        };
    }
}
