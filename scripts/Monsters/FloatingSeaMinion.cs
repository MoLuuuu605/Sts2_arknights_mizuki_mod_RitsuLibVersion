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

public sealed class FloatingSeaMinion : MinionModel
{
    public const string VisualsScenePath = "res://Arknights_Mizuki/monsters/floater.tscn";

    public override int MinInitialHp => 4;
    public override int MaxInitialHp => 6;

    protected override string VisualsPath => VisualsScenePath;
    public override IEnumerable<string> AssetPaths => [VisualsScenePath];

    public override CreatureAnimator GenerateAnimator(MegaSprite spine)
    {
        var idle = new AnimState("Idle_01", true);
        var attack = new AnimState("Attack_01", false) { NextState = idle };
        var die = new AnimState("Die_01", false);

        var animator = new CreatureAnimator(idle, spine);
        animator.AddAnyState(CreatureAnimator.attackTrigger, attack, () => true);
        animator.AddAnyState(CreatureAnimator.deathTrigger, die, () => true);
        return animator;
    }

    public override async Task OnSummon(PlayerChoiceContext choiceContext,Player owner, MinionSummonOptions options)
    {
        decimal maxHp = options.MaxHp ?? MaxInitialHp;
        await CreatureCmd.SetMaxAndCurrentHp(this.Creature, maxHp);
        await CreatureCmd.SetCurrentHp(this.Creature, Math.Min(3m, maxHp));

        // 基础格挡数值（可通过其他方式增加）

        await PowerCmd.Apply<FloatingSeaBlockPower>(choiceContext,this.Creature,1,this.Creature,null);
        await PowerCmd.Apply<FloatingSeaBlockAction>(choiceContext,this.Creature,1,this.Creature,null);
        Entry.Logger.Info($"[MinionAction] FloatingSeaMinion summoned ownerNetId={owner.NetId} creature={Creature.Name} powers={DescribePowers()}");
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
