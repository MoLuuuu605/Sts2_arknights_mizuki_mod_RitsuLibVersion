using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MinionLib.Minion;

namespace Arknights_Mizuki.Scripts.Patches;

[HarmonyPatch(typeof(NCreature), "UpdateBounds", typeof(Node))]
public static class SpineCreatureBoundsPatch
{
    private static readonly FieldInfo StateDisplayField = AccessTools.Field(typeof(NCreature), "_stateDisplay");
    private static readonly FieldInfo SelectionReticleField = AccessTools.Field(typeof(NCreature), "_selectionReticle");

    public static bool Prefix(NCreature __instance, Node boundsContainer)
    {
        if (!UsesScriptedSpineVisual(__instance))
            return true;

        Control? bounds = boundsContainer.GetNodeOrNull<Control>("%Bounds") ?? __instance.Visuals.GetNodeOrNull<Control>("%Bounds");
        if (bounds == null)
            return true;

        bounds.MouseFilter = Control.MouseFilterEnum.Ignore;

        Rect2 rect = bounds.GetGlobalRect();
        Vector2 position = rect.Position;
        Vector2 size = rect.Size;

        if (size.X < 0)
        {
            position.X += size.X;
            size.X = -size.X;
        }

        if (size.Y < 0)
        {
            position.Y += size.Y;
            size.Y = -size.Y;
        }

        __instance.Hitbox.GlobalPosition = position;
        __instance.Hitbox.Size = size;
        __instance.Hitbox.Scale = Vector2.One;
        if (__instance.Entity.Monster is MinionModel &&
            __instance.Entity.PetOwner != null &&
            LocalContext.IsMe(__instance.Entity.PetOwner))
        {
            __instance.Hitbox.MouseFilter = Control.MouseFilterEnum.Stop;
        }

        if (SelectionReticleField.GetValue(__instance) is Control selectionReticle)
        {
            selectionReticle.GlobalPosition = position;
            selectionReticle.Size = size;
            selectionReticle.Scale = Vector2.One;
            selectionReticle.PivotOffset = selectionReticle.Size * 0.5f;
        }

        Marker2D? intentPos = boundsContainer.GetNodeOrNull<Marker2D>("%IntentPos") ?? __instance.Visuals.GetNodeOrNull<Marker2D>("%IntentPos");
        if (intentPos != null)
            __instance.IntentContainer.Position = intentPos.Position - __instance.IntentContainer.Size * 0.5f;

        if (StateDisplayField.GetValue(__instance) is NCreatureStateDisplay stateDisplay)
            stateDisplay.SetCreatureBounds(__instance.Hitbox);

        return false;
    }

    private static bool UsesScriptedSpineVisual(NCreature creature)
    {
        Node? visual = creature.Visuals.GetNodeOrNull<Node>("%Visuals") ?? creature.Visuals.GetNodeOrNull<Node>("%SpineSprite");
        return visual != null && visual.HasMethod("play_trigger");
    }
}
