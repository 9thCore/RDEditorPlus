using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDLevelEditor;
using System;
using System.Reflection;
using UnityEngine;

namespace RDEditorPlus.Patch.Select.MultiEdit
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

            private static void Postfix(Property __result)
            {
                BasePropertyInfo propertyInfo = __result.propertyInfo;

                if (propertyInfo is NullablePropertyInfo nullablePropertyInfo)
                {
                    InspectorPanel inspectorPanel = __result.control.inspectorPanel;

                    __result.button.onClick.RemoveAllListeners();
                    __result.button.onClick.AddListener(() =>
                    {
                        LevelEventControl_Base currentControl = scnEditor.instance.selectedControls[0];

                        bool flag = nullablePropertyInfo.IsUsed(currentControl.levelEvent);
                        object obj = flag ? null : nullablePropertyInfo.toggleOnValue;

                        foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                        {
                            propertyInfo.propertyInfo.SetValue(eventControl.levelEvent, obj);
                        }

                        scrConductor.PlayImmediately(flag ? "sndButtonClick" : "sndButtonClickOn",
                            group: RDUtils.GetMixerGroup("LevelEditorInspectorPanel"), ignoreListenerPause: true);

                        __result.UpdateUI(currentControl.levelEvent);
                        currentControl.SaveAndUpdateUI();
                    });
                }
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
