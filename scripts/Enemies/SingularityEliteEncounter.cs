using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Arknights_Mizuki.Scripts.Acts;

namespace Arknights_Mizuki.Scripts.Enemies;

[RegisterActEncounter(typeof(EvolutionSingularityAct))]
public sealed class SingularityEliteEncounter : ModEncounterTemplate
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
    {
        ModelDb.Monster<ColdDisaster>()
    };

    public override string BossNodePath => "res://Arknights_Mizuki/images/ui/run_history/cold_disaster_elite_icon";

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
            (ModelDb.Monster<ColdDisaster>().ToMutable(), null)
        };
    }
}
