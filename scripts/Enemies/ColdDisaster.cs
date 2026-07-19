using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Cards;
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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Godot;

namespace Arknights_Mizuki.Scripts.Enemies;


[RegisterMonster]
public sealed class ColdDisaster : ModMonsterTemplate
{
	private const string DebuffMoveId = "COLD_DISASTER_DEBUFF";
	private const string MultiAttackMoveId = "COLD_DISASTER_MULTI";
	private const string HeavyMoveId = "COLD_DISASTER_HEAVY";
	private const string FreezeMoveId = "COLD_DISASTER_FREEZE";

	public override int MinInitialHp => Asc(200, 175);
	public override int MaxInitialHp => Asc(230, 205);
	public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath:"res://Arknights_Mizuki/enemies/cold_disaster/cold_disaster.tscn"
    );

	protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);
	public override CreatureAnimator GenerateAnimator(MegaSprite controller) => SpineAnimatorFactory.Create(controller);

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ColdWavePower>(new ThrowingPlayerChoiceContext(), Creature, ColdWavePower.HitsPerCold, Creature, null, false);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState debuff = new(
			DebuffMoveId,
			targets => ApplyVulnerableAndCold(targets),
			new DebuffIntent(true))
		{
			FollowUpStateId = MultiAttackMoveId
		};

		MoveState multi = new(
			MultiAttackMoveId,
			targets => MultiAttack(targets),
			new MultiAttackIntent(Asc(6, 5), 4))
		{
			FollowUpStateId = HeavyMoveId
		};

		MoveState heavy = new(
			HeavyMoveId,
			targets => HeavyAttackAndStrength(targets),
			new SingleAttackIntent(Asc(24, 20)),
			new BuffIntent())
		{
			FollowUpStateId = FreezeMoveId
		};

		MoveState freeze = new(
			FreezeMoveId,
			_ => AddFrozenCards(),
			new DebuffIntent(true))
		{
			FollowUpStateId = DebuffMoveId
		};

		return new MonsterMoveStateMachine(
			new MonsterState[] { debuff, multi, heavy, freeze },
			debuff);
	}

	private async Task ApplyVulnerableAndCold(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(Creature, "Cast", 0.25f);
		PlayerChoiceContext choiceContext = new ThrowingPlayerChoiceContext();
		foreach (Creature target in targets.Where(target => target.IsAlive))
		{
			await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 2, Creature, null, false);
			await PowerCmd.Apply<ColdPower>(choiceContext, target, Asc(2, 1), Creature, null, false);
		}
	}

	private async Task MultiAttack(IReadOnlyList<Creature> targets)
	{
		Creature? target = targets.FirstOrDefault(target => target.IsAlive);
		if (target == null)
			return;

		await CreatureCmd.TriggerAnim(Creature, "Attack", 0.25f);
		int damage = Asc(6, 5);
		for (int i = 0; i < 4; i++)
		{
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, damage, ValueProp.Move, Creature);
		}
	}

	private async Task HeavyAttackAndStrength(IReadOnlyList<Creature> targets)
	{
		Creature? target = targets.FirstOrDefault(target => target.IsAlive);
		if (target != null)
		{
			await CreatureCmd.TriggerAnim(Creature, "Attack", 0.25f);
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, Asc(24, 20), ValueProp.Move, Creature);
		}

		await CreatureCmd.TriggerAnim(Creature, "Buff", 0.2f);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, Asc(3, 2), Creature, null, false);
	}

	private async Task AddFrozenCards()
	{
		await CreatureCmd.TriggerAnim(Creature, "Cast", 0.25f);
		foreach (var player in Creature.CombatState.Players.Where(player => player.Creature.IsAlive))
		{
			int count = Asc(3,2);
			for (int i = 0; i < count; i++)
			{
				CardModel frozen = Creature.CombatState.CreateCard<Frozen>(player);
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
					frozen,
					PileType.Draw,
					player,
					CardPilePosition.Random));
			}
		}
	}

	private static int Asc(int highAscensionValue, int normalValue)
	{
		return AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, highAscensionValue, normalValue);
	}
}
