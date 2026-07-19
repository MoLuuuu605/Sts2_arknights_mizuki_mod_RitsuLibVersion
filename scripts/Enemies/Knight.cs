using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Godot;

namespace Arknights_Mizuki.Scripts.Enemies;


[RegisterMonster]
public class Knight : ModMonsterTemplate
{
    private const string OpenMoveId = "KNIGHT_OPEN";
    private const string PhaseOneAId = "KNIGHT_PHASE_1_A";
    private const string PhaseOneBId = "KNIGHT_PHASE_1_B";
    private const string PhaseOneCId = "KNIGHT_PHASE_1_C";
    private const string PhaseOneDId = "KNIGHT_PHASE_1_D";
    private const string PhaseTwoStartId = "KNIGHT_PHASE_2_START";
    private const string PhaseTwoAId = "KNIGHT_PHASE_2_A";
    private const string PhaseTwoBId = "KNIGHT_PHASE_2_B";
    private const string PhaseTwoCId = "KNIGHT_PHASE_2_C";
    private const string PhaseTwoDId = "KNIGHT_PHASE_2_D";

    private bool phaseTwoTriggered;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 280, 260);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 320, 300);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://Arknights_Mizuki/enemies/last_knight/knight.tscn"
    );
    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);
    public override bool ShouldReceiveCombatHooks => true;

    public override CreatureAnimator GenerateAnimator(MegaSprite controller) => SpineAnimatorFactory.Create(controller, cast: "Skill_1_Begin", buff: "Skill_1_Begin", summon: "Skill_1_Begin");


    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        phaseTwoTriggered = false;

        MoveState open = new(OpenMoveId, _ => ApplyToricToughness(28), new BuffIntent()) { FollowUpStateId = PhaseOneAId };
        MoveState p1a = new(PhaseOneAId, targets => AttackAndCold(targets, Damage(11, 15, 13, 17), 1), new SingleAttackIntent(() => DamageIntent(16, 20, 19, 26)), new DebuffIntent(false)) { FollowUpStateId = PhaseOneBId };
        MoveState p1b = new(PhaseOneBId, targets => Attack(targets, Damage(18, 26, 22, 30)), new SingleAttackIntent(() => DamageIntent(22, 30, 26, 34))) { FollowUpStateId = PhaseOneCId };
        MoveState p1c = new(PhaseOneCId, _ => BuffSelf(ToricToughness: Asc(4, 3), Strength: Asc(2, 1)), new BuffIntent()) { FollowUpStateId = PhaseOneDId };
        MoveState p1d = new(PhaseOneDId, targets => DebuffPlayer(targets, vulnerable: 2, weak: 2, cold: 1), new DebuffIntent(true)) { FollowUpStateId = PhaseOneAId };
        MoveState phaseTwoStart = new(PhaseTwoStartId, _ => EnterPhaseTwo(), new UnknownIntent()) { FollowUpStateId = PhaseTwoAId };
        MoveState p2a = new(PhaseTwoAId, targets => AttackAndDebuff(targets, Damage(14, 18, 17, 21), vulnerable: 2), new SingleAttackIntent(() => DamageIntent(18, 22, 21, 27)), new DebuffIntent(false)) { FollowUpStateId = PhaseTwoBId };
        MoveState p2b = new(PhaseTwoBId, targets => Attack(targets, Damage(26, 34, 30, 38)), new SingleAttackIntent(() => DamageIntent(28, 34, 32, 38))) { FollowUpStateId = PhaseTwoCId };
        MoveState p2c = new(PhaseTwoCId, _ => BuffSelf(ToricToughness: Asc(8, 7), Strength: 3), new BuffIntent()) { FollowUpStateId = PhaseTwoDId };
        MoveState p2d = new(PhaseTwoDId, targets => DebuffPlayer(targets, vulnerable: 2, weak: 2, cold: 2), new DebuffIntent(true)) { FollowUpStateId = PhaseTwoAId };

        return new MonsterMoveStateMachine(
            new MonsterState[] { open, p1a, p1b, p1c, p1d, phaseTwoStart, p2a, p2b, p2c, p2d },
            open);
    }
    public override async Task AfterAddedToRoom()
    {
        var cc=new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<PlatingPower>(cc,this.Creature,20,this.Creature,null);
        await PowerCmd.Apply<ColdWavePower>(cc, Creature, ColdWavePower.HitsPerCold, Creature, null, false);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, MegaCrit.Sts2.Core.Models.CardModel cardSource)
    {
        if (target != Creature || phaseTwoTriggered || Creature.CurrentHp > Creature.MaxHp / 2m || !Creature.IsAlive)
            return;

        phaseTwoTriggered = true;
        await CreatureCmd.GainBlock(this.Creature,999,ValueProp.Unpowered,null,true);
        SetMoveImmediate(MoveStateMachine.States[PhaseTwoStartId] as MoveState, true);
    }

    private async Task Attack(IReadOnlyList<Creature> targets, int damage)
    {
        Creature? target = targets.FirstOrDefault();
        if (target == null)
            return;

        await CreatureCmd.TriggerAnim(Creature, "Attack", 0.25f);
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, damage, ValueProp.Move, Creature);
    }

    private async Task AttackAndCold(IReadOnlyList<Creature> targets, int damage, decimal cold)
    {
        await Attack(targets, damage);
        await DebuffPlayer(targets, vulnerable: 0, weak: 0, cold);
    }

    private async Task AttackAndDebuff(IReadOnlyList<Creature> targets, int damage, decimal vulnerable)
    {
        await Attack(targets, damage);
        await DebuffPlayer(targets, vulnerable, weak: 0, cold: 0);
    }

    private async Task DebuffPlayer(IReadOnlyList<Creature> targets, decimal vulnerable, decimal weak, decimal cold)
    {
        Creature? target = targets.FirstOrDefault();
        if (target == null)
            return;

        ThrowingPlayerChoiceContext choiceContext = new();
        if (vulnerable > 0)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, vulnerable, Creature, null, false);
        if (weak > 0)
            await PowerCmd.Apply<WeakPower>(choiceContext, target, weak, Creature, null, false);
        if (cold > 0)
            await PowerCmd.Apply<ColdPower>(choiceContext, target, cold, Creature, null, false);
    }

    private async Task BuffSelf(decimal ToricToughness, decimal Strength)
    {
        ThrowingPlayerChoiceContext choiceContext = new();
        if (ToricToughness > 0)
            await ApplyToricToughness(ToricToughness);
        if (Strength > 0)
            await PowerCmd.Apply<StrengthPower>(choiceContext, Creature, Strength, Creature, null, false);
    }

    private async Task ApplyToricToughness(decimal amount)
    {
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, amount, Creature, null, false);
    }

    private async Task EnterPhaseTwo()
    {
        foreach (PowerModel power in Creature.Powers.ToList())
        {
            if (power.Type is PowerType.Buff or PowerType.Debuff)
                await PowerCmd.Remove(power);
        }

        await CreatureCmd.Heal(Creature, Creature.MaxHp / 2m, true);
        await PowerCmd.Apply<ColdWavePower>(choiceContext:new ThrowingPlayerChoiceContext(), Creature, ColdWavePower.HitsPerCold, Creature, null, false);
    }

    private int Damage(int normalMin, int normalMax, int ascMin, int ascMax)
    {
        return RunRng.MonsterAi.NextInt(Asc(ascMin, normalMin), Asc(ascMax, normalMax) + 1);
    }

    private int DamageIntent(int normalMin, int normalMax, int ascMin, int ascMax)
    {
        return (Asc(ascMin, normalMin) + Asc(ascMax, normalMax)) / 2;
    }

    private static int Asc(int highAscensionValue, int normalValue)
    {
        return AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, highAscensionValue, normalValue);
    }
}
