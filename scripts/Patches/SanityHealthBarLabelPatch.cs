using System.Reflection;
using Arknights_Mizuki.Scripts.Powers;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(NHealthBar), "RefreshText")]
public static class SanityHealthBarLabelPatch
{
    private static readonly FieldInfo CreatureField = AccessTools.Field(typeof(NHealthBar), "_creature");
    private static readonly FieldInfo HpLabelField = AccessTools.Field(typeof(NHealthBar), "_hpLabel");

    private static void Postfix(NHealthBar __instance)
    {
        if (CreatureField.GetValue(__instance) is not Creature creature || !creature.IsAlive)
            return;

        if (!creature.HasPower<SanityPower>())
            return;

        SanityPower sanityPower = creature.GetPower<SanityPower>();
        int damage = sanityPower.GetForecastDamage();
        if (damage <= 0)
            return;

        if (HpLabelField.GetValue(__instance) is not MegaLabel hpLabel)
            return;

        int currentHp = (int)Math.Ceiling((decimal)creature.CurrentHp);
        int maxHp = (int)Math.Ceiling((decimal)creature.MaxHp);
        int predictedHp = Math.Max(0, currentHp - damage);
        hpLabel.SetTextAutoSize($"{currentHp}({predictedHp})/{maxHp}");
    }
}
