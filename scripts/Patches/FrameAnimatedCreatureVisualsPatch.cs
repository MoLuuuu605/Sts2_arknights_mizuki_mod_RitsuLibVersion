using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
public static class FrameAnimatedCreatureVisualsTriggerPatch
{
	public static void Postfix(NCreature __instance, string trigger)
	{
		PlayTrigger(__instance, trigger);
	}

	public static void PlayTrigger(NCreature creature, string trigger)
	{
		if (TryPlayTrigger(creature.Visuals.GetNodeOrNull<Node>("%Visuals"), trigger))
		return;

		TryPlayTrigger(creature.Visuals.GetNodeOrNull<Node>("%SpineSprite"), trigger);
	}

	private static bool TryPlayTrigger(Node? node, string trigger)
	{
		if (node == null || !node.HasMethod("play_trigger"))
		return false;

		node.Call("play_trigger", trigger);
		return true;
	}
}

[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
public static class FrameAnimatedCreatureVisualsDeathPatch
{
	public static void Prefix(NCreature __instance)
	{
		FrameAnimatedCreatureVisualsTriggerPatch.PlayTrigger(__instance, "Dead");
	}
}
