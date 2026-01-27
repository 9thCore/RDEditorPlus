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
    internal static class Patch_PropertyControl_ExpPositionPicker
    {
        [HarmonyPatch(typeof(PropertyControl_ExpPositionPicker), nameof(PropertyControl_ExpPositionPicker.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_ExpPositionPicker __instance)
            {
                if (!__instance.EqualValueForSelectedEvents(out bool x, out bool y))
                {
                    if (!x)
                    {
                        __instance.expPositionPicker.x.text = string.Empty;
                        PropertyLastXString[__instance] = string.Empty;
                        ((Text)__instance.expPositionPicker.x.placeholder).text = InspectorUtil.MixedTextShorter;
                    }
                    else
                    {
                        ((Text)__instance.expPositionPicker.x.placeholder).text = InspectorUtil.DefaultNullText;
                    }

                    if (!y)
                    {
                        __instance.expPositionPicker.y.text = string.Empty;
                        PropertyLastYString[__instance] = string.Empty;
                        ((Text)__instance.expPositionPicker.y.placeholder).text = InspectorUtil.MixedTextShorter;
                    }
                    else
                    {
                        ((Text)__instance.expPositionPicker.y.placeholder).text = InspectorUtil.DefaultNullText;
                    }

                    return false;
                }

                ((Text)__instance.expPositionPicker.x.placeholder).text = InspectorUtil.DefaultNullText;
                ((Text)__instance.expPositionPicker.y.placeholder).text = InspectorUtil.DefaultNullText;

                PropertyLastXString[__instance] = __instance.expPositionPicker.x.text;
                PropertyLastYString[__instance] = __instance.expPositionPicker.y.text;

                return true;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_ExpPositionPicker), nameof(PropertyControl_ExpPositionPicker.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_ExpPositionPicker __instance, InspectorPanel.ChangeAction action)
            {
                __instance.expPositionPicker.x.onEndEdit.RemoveAllListeners();
                __instance.expPositionPicker.y.onEndEdit.RemoveAllListeners();

                __instance.expPositionPicker.x.onEndEdit.AddListener(text =>
                {
                    string sound = __instance.expPositionPicker.PlayOnEndEdit ? "sndEditorValueChange" : null;

                    if (PropertyLastXString.TryGetValue(__instance, out string oldText) && oldText == text)
                    {
                        if (sound != null)
                        {
                            __instance.inspectorPanel.LevelEditorPlaySound(sound, __instance.inspectorPanel.defaultGroup);
                        }

                        return;
                    }

                    ((Text)__instance.expPositionPicker.x.placeholder).text = InspectorUtil.DefaultNullText;
                    PropertyLastXString[__instance] = text;
                    PropertyData[__instance] = PropertyUpdateType.UpdateX;
                    action(null, sound, null);
                });

                __instance.expPositionPicker.y.onEndEdit.AddListener(text =>
                {
                    string sound = __instance.expPositionPicker.PlayOnEndEdit ? "sndEditorValueChange" : null;

                    if (PropertyLastYString.TryGetValue(__instance, out string oldText) && oldText == text)
                    {
                        if (sound != null)
                        {
                            __instance.inspectorPanel.LevelEditorPlaySound(sound, __instance.inspectorPanel.defaultGroup);
                        }

                        return;
                    }

                    ((Text)__instance.expPositionPicker.y.placeholder).text = InspectorUtil.DefaultNullText;
                    PropertyLastYString[__instance] = text;
                    PropertyData[__instance] = PropertyUpdateType.UpdateY;
                    action(null, sound, null);
                });

                __instance.expPositionPicker.units.GetComponent<Button>().onClick.AddListener(() =>
                {
                    PropertyLastXString[__instance] = __instance.expPositionPicker.x.text;
                    PropertyLastYString[__instance] = __instance.expPositionPicker.y.text;
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_ExpPositionPicker), nameof(PropertyControl_ExpPositionPicker.Save))]
        private static class Save
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(instruction => instruction.MatchLdarg(1))
                    .GotoNext(MoveType.After, instruction => instruction.MatchLdloc(0))
                    .Emit(OpCodes.Ldarg_0)
                    .Emit(OpCodes.Ldarg_1)
                    .EmitDelegate((FloatExpression2 floatExpression2, PropertyControl_ExpPositionPicker propertyControl, LevelEvent_Base levelEvent) =>
                    {
                        PropertyUpdateType type = GetUpdateType(propertyControl);
                        bool applyX = type.HasFlag(PropertyUpdateType.UpdateX);
                        bool applyY = type.HasFlag(PropertyUpdateType.UpdateY);

                        if (applyX && applyY)
                        {
                            return floatExpression2;
                        }

                        FloatExpression2 currentValue = (FloatExpression2)propertyControl.GetEventValue(levelEvent);

                        if (applyX && !applyY)
                        {
                            return new FloatExpression2(floatExpression2.x, currentValue.y);
                        }

                        return new FloatExpression2(currentValue.x, floatExpression2.y);
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
