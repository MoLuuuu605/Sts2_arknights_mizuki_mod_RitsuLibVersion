using Arknights_Mizuki.Scripts.Actions;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Minion;

namespace Arknights_Mizuki.Scripts.Minions;
public sealed class HarvestMinion : MinionModel
{
    public const string VisualsScenePath = "res://Arknights_Mizuki/monsters/harvest.tscn";

    public override int MinInitialHp => 4;
    public override int MaxInitialHp => 8;
    protected override string VisualsPath => VisualsScenePath;
    public override IEnumerable<string> AssetPaths => [VisualsScenePath];
    public int baseDamage = 3;

    public override CreatureAnimator GenerateAnimator(MegaSprite spine)
    {
        var idle = new AnimState("Idle", true);
        var skillIdle = new AnimState("Skill_Idle", true);
        var skillBegin = new AnimState("Skill_Begin", false) { NextState = skillIdle };
        var skillAttack = new AnimState("Skill_Attack", false) { NextState = skillIdle };
        var die = new AnimState("Die", false);

        var animator = new CreatureAnimator(idle, spine);
        animator.AddAnyState(CreatureAnimator.powerUpTrigger, skillBegin, () => true);
        animator.AddAnyState(CreatureAnimator.attackTrigger, skillAttack, () => true);
        animator.AddAnyState(CreatureAnimator.deathTrigger, die, () => true);
        return animator;
    }

    public override async Task OnSummon(PlayerChoiceContext choiceContext,Player owner, MinionSummonOptions options)
    {
        decimal maxHp = options.MaxHp ?? MaxInitialHp;
        await CreatureCmd.SetMaxAndCurrentHp(this.Creature, maxHp);
        await CreatureCmd.SetCurrentHp(this.Creature, Math.Min(4m, maxHp));
        await PowerCmd.Apply<HarvestAttackPower>(choiceContext,this.Creature,1,this.Creature,null);
        await PowerCmd.Apply<HarvestAttackAction>(choiceContext,this.Creature,1,this.Creature,null);
        Entry.Logger.Info($"[MinionAction] HarvestMinion summoned ownerNetId={owner.NetId} creature={Creature.Name} powers={DescribePowers()}");
    }

    private string DescribePowers()
    {
        return string.Join(", ", Creature.Powers.Select(DescribePower));
    }

    private static string DescribePower(PowerModel power)
    {
        return $"{power.Id.Entry}:{power.Amount}:{power.GetType().Name}";
    }


}
