using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Arknights_Mizuki.Scripts.Utils;

public static class MinionAnimationHelper
{
    private static readonly HashSet<Creature> HarvestSkillBegun = [];

    public static void Play(Creature? creature, string trigger)
    {
        if (creature == null)
            return;

        NCombatRoom.Instance?.GetCreatureNode(creature)?.SetAnimationTrigger(trigger);
    }

    public static bool MarkHarvestSkillBegun(Creature creature)
    {
        return HarvestSkillBegun.Add(creature);
    }
}
