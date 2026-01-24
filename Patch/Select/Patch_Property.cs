using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDLevelEditor;
using System;
using System.Reflection;
using UnityEngine;

namespace RDEditorPlus.Patch.Select
{
    internal static class Patch_Property
    {
        [HarmonyPatch(typeof(Property), nameof(Property.Save))]
        private static class Save
        {
            private static bool Prefix(Property __instance, LevelEvent_Base levelEvent)
            {
                if (!PluginConfig.SelectionMultiEditEnabled)
                {
                    return true;
                }

                if (PropertyStorage.Instance.HasChanged(__instance))
                {
                    Plugin.LogInfo($"(Save) Detected change: {__instance.gameObject}");
                    return true;
                }

                // The property has not changed, but we still want to update its visibility
                __instance.UpdateControlVisibility(levelEvent);
                return false;
            }
        }

        [HarmonyPatch(typeof(Property), nameof(Property.Create))]
        private static class Create
        {
            private static void ILManipulator(ILContext il)
            {
                if (!PluginConfig.SelectionMultiEditEnabled)
                {
                    return;
                }

                MethodInfo delegateInjector = typeof(Create).GetMethod(nameof(InjectDelegate), BindingFlags.NonPublic | BindingFlags.Static);

                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchNewobj<InspectorPanel.ChangeAction>())
                    .Emit(OpCodes.Ldloc_1)
                    .Emit(OpCodes.Call, delegateInjector);
            }

            private static InspectorPanel.ChangeAction InjectDelegate(InspectorPanel.ChangeAction action, GameObject gameObject)
            {
                return ConstructDelegate(action, gameObject.GetComponent<Property>());
            }

            private static InspectorPanel.ChangeAction ConstructDelegate(InspectorPanel.ChangeAction action, Property property)
            {
                return (Action extraActions, string sound = null, string group = null) =>
                {
                    Plugin.LogInfo($"(Delegate) Detected change: {property.gameObject}");
                    PropertyStorage.Instance.MarkChanged(property);
                    action(extraActions, sound, group);
                };
            }
        }
    }
}
