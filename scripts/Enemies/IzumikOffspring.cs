using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Godot;

namespace Arknights_Mizuki.Scripts.Enemies;


[RegisterMonster]
public sealed class IzumikOffspring : ModMonsterTemplate
{
    private static int Asc(int highAscensionValue, int normalValue)
    {
        return AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, highAscensionValue, normalValue);
    }
    private const string AttackAndSlimeMoveId = "IZUMIK_OFFSPRING_ATTACK_AND_SLIME";
    private const string ExplodeSacrificeMoveId = "IZUMIK_OFFSPRING_EXPLODE_SACRIFICE";
    private const int AttackDamage = 7;

    public override int MinInitialHp => Asc(34,30);
    public override int MaxInitialHp => Asc(42,34);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath:"res://Arknights_Mizuki/enemies/son_of_izumik/son_of_izumik.tscn"
    );
    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

    public override CreatureAnimator GenerateAnimator(MegaSprite controller) => SpineAnimatorFactory.Create(controller, attack: "Move", cast: "Move", buff: "Move", summon: "Move");

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null, false);
        await PowerCmd.Apply<OffspringSacrificePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null, false);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attackAndSlime = new(
            AttackAndSlimeMoveId,
            targets => AttackAndSlime(targets),
            new SingleAttackIntent(AttackDamage),
            new StatusIntent(1))
        {
            FollowUpStateId = ExplodeSacrificeMoveId
        };

        MoveState explodeSacrifice = new(
            ExplodeSacrificeMoveId,
            targets => ExplodeThenSacrifice(targets),
            new DeathBlowIntent(() => Math.Max(1, (int)Math.Ceiling((decimal)Creature.CurrentHp))),
            new BuffIntent())
        {
            FollowUpStateId = AttackAndSlimeMoveId
        };

        return new MonsterMoveStateMachine(
            new MonsterState[] { attackAndSlime, explodeSacrifice },
            attackAndSlime);
    }

    private async Task AttackAndSlime(IReadOnlyList<Creature> targets)
    {
        Creature? target = targets.FirstOrDefault();
        if (target != null)
        {
            await CreatureCmd.TriggerAnim(Creature, "Attack", 0.2f);
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, AttackDamage, ValueProp.Move, Creature);
        }

        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.15f);
        await AddSlimed(target);
    }

    private async Task ExplodeThenSacrifice(IReadOnlyList<Creature> targets)
    {
        Creature? target = targets.FirstOrDefault();
        decimal damage = Math.Max(1m, Creature.CurrentHp);
        if (target != null)
        {
            await CreatureCmd.TriggerAnim(Creature, "Attack", 0.2f);
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, damage, ValueProp.Move, Creature);
        }

        await SacrificeToIzumik();
    }

    private async Task AddSlimed(Creature? target)
    {
        if (target?.Player != null)
        {
            CardModel slimed = Creature.CombatState.CreateCard<Slimed>(target.Player);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                slimed,
                PileType.Discard,
                target.Player,
                CardPilePosition.Bottom));
        }
    }

    private async Task SacrificeToIzumik()
    {
        PlayerChoiceContext choiceContext = new ThrowingPlayerChoiceContext();
        Izumik? izumik = Creature.CombatState.Enemies
            .Select(enemy => enemy.Monster)
            .OfType<Izumik>()
            .FirstOrDefault(boss => boss.Creature.IsAlive);

        if (izumik != null)
        {
            await izumik.AbsorbOffspring(choiceContext, Creature);
        }

        if (Creature.IsAlive)
        {
            await CreatureCmd.Damage(
                choiceContext,
                Creature,
                Creature.CurrentHp,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                Creature);
        }
    }
}
