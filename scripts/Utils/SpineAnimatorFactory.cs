using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace Arknights_Mizuki.Scripts.Utils;

public static class SpineAnimatorFactory
{
    public static CreatureAnimator Create(
        MegaSprite controller,
        string idle = "Idle",
        string? attack = "Attack",
        string? cast = "Skill_1",
        string? buff = "Skill_2",
        string? summon = "Skill_1",
        string? die = "Die",
        string? hit = null)
    {
        AnimState idleState = new(idle, isLooping: true);
        CreatureAnimator animator = new(idleState, controller);
        animator.AddAnyState("Idle", idleState);

        AddState(animator, "Attack", attack, idleState);
        AddState(animator, "Cast", cast, idleState);
        AddState(animator, "Buff", buff, idleState);
        AddState(animator, "Summon", summon, idleState);
        AddState(animator, "Dead", die, null);
        AddState(animator, "Hit", hit, idleState);

        return animator;
    }

    private static void AddState(CreatureAnimator animator, string trigger, string? animation, AnimState? nextState)
    {
        if (string.IsNullOrEmpty(animation))
            return;

        AnimState state = new(animation);
        state.NextState = nextState;
        animator.AddAnyState(trigger, state);
    }
}
