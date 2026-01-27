using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_PositionPicker
    {
        [HarmonyPatch(typeof(PropertyControl_PositionPicker), nameof(PropertyControl_PositionPicker.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_PositionPicker __instance)
            {
                if (!__instance.EqualValueForSelectedEvents(out bool x, out bool y))
                {
                    if (!x)
                    {
                        __instance.positionPicker.x.text = string.Empty;
                        PropertyLastXString[__instance] = string.Empty;
                        ((Text)__instance.positionPicker.x.placeholder).text = InspectorUtil.MixedTextShorter;
                    }
                    else
                    {
                        ((Text)__instance.positionPicker.x.placeholder).text = InspectorUtil.DefaultNullText;
                    }

                    if (!y)
                    {
                        __instance.positionPicker.y.text = string.Empty;
                        PropertyLastYString[__instance] = string.Empty;
                        ((Text)__instance.positionPicker.y.placeholder).text = InspectorUtil.MixedTextShorter;
                    }
                    else
                    {
                        ((Text)__instance.positionPicker.y.placeholder).text = InspectorUtil.DefaultNullText;
                    }

                    return false;
                }

                ((Text)__instance.positionPicker.x.placeholder).text = InspectorUtil.DefaultNullText;
                ((Text)__instance.positionPicker.y.placeholder).text = InspectorUtil.DefaultNullText;

                PropertyLastXString[__instance] = __instance.positionPicker.x.text;
                PropertyLastYString[__instance] = __instance.positionPicker.y.text;

                return true;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_PositionPicker), nameof(PropertyControl_PositionPicker.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_PositionPicker __instance, InspectorPanel.ChangeAction action)
            {
                __instance.positionPicker.x.onEndEdit.RemoveAllListeners();
                __instance.positionPicker.y.onEndEdit.RemoveAllListeners();

                __instance.positionPicker.x.onEndEdit.AddListener(text =>
                {
                    string sound = __instance.positionPicker.PlayOnEndEdit ? "sndEditorValueChange" : null;

                    if (PropertyLastXString.TryGetValue(__instance, out string oldText) && oldText == text)
                    {
                        if (sound != null)
                        {
                            __instance.inspectorPanel.LevelEditorPlaySound(sound, __instance.inspectorPanel.defaultGroup);
                        }

                        return;
                    }

                    ((Text)__instance.positionPicker.x.placeholder).text = InspectorUtil.DefaultNullText;
                    PropertyLastXString[__instance] = text;
                    PropertyData[__instance] = PropertyUpdateType.UpdateX;
                    action(null, sound, null);
                });

                __instance.positionPicker.y.onEndEdit.AddListener(text =>
                {
                    string sound = __instance.positionPicker.PlayOnEndEdit ? "sndEditorValueChange" : null;

                    if (PropertyLastYString.TryGetValue(__instance, out string oldText) && oldText == text)
                    {
                        if (sound != null)
                        {
                            __instance.inspectorPanel.LevelEditorPlaySound(sound, __instance.inspectorPanel.defaultGroup);
                        }

                        return;
                    }

                    ((Text)__instance.positionPicker.y.placeholder).text = InspectorUtil.DefaultNullText;
                    PropertyLastYString[__instance] = text;
                    PropertyData[__instance] = PropertyUpdateType.UpdateY;
                    action(null, sound, null);
                });

                __instance.positionPicker.units.GetComponent<Button>().onClick.AddListener(() =>
                {
                    PropertyLastXString[__instance] = __instance.positionPicker.x.text;
                    PropertyLastYString[__instance] = __instance.positionPicker.y.text;
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_PositionPicker), nameof(PropertyControl_PositionPicker.Save))]
        private static class Save
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchBox(typeof(Float2)))
                    .Emit(OpCodes.Ldarg_0)
                    .Emit(OpCodes.Ldarg_1)
                    .EmitDelegate((Float2 float2, PropertyControl_PositionPicker propertyControl, LevelEvent_Base levelEvent) =>
                    {
                        PropertyUpdateType type = GetUpdateType(propertyControl);
                        bool applyX = type.HasFlag(PropertyUpdateType.UpdateX);
                        bool applyY = type.HasFlag(PropertyUpdateType.UpdateY);

                        if (applyX && applyY)
                        {
                            return float2;
                        }

                        Float2 currentValue = (Float2)propertyControl.GetEventValue(levelEvent);

                        if (applyX && !applyY)
                        {
                            return Float2Util.Combine(float2, currentValue);
                        }

                        return Float2Util.Combine(currentValue, float2);
                    });
            }
        }

        private static PropertyUpdateType GetUpdateType(PropertyControl property)
        {
            return PropertyData.TryGetValue(property, out PropertyUpdateType type) ? type : PropertyUpdateType.UpdateBoth;
        }

        private static readonly Dictionary<PropertyControl, PropertyUpdateType> PropertyData = new();
        private static readonly Dictionary<PropertyControl, string> PropertyLastXString = new();
        private static readonly Dictionary<PropertyControl, string> PropertyLastYString = new();

        [Flags]
        private enum PropertyUpdateType
        {
            UpdateX = 1,
            UpdateY = 2,
            UpdateBoth = UpdateX | UpdateY
        }
    }
}
