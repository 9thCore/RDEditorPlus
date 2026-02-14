using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_SetRoomPerspective
    {

        [HarmonyPatch(typeof(PropertyControl_SetRoomPerspective), nameof(PropertyControl_SetRoomPerspective.UpdateUI))]
        private static class UpdateUI
        {
            private static void Prefix(PropertyControl_SetRoomPerspective __instance, ref bool __runOriginal)
            {
                __runOriginal = true;

                for (int i = 0; i < __instance.positionPickers.Length; i++)
                {
                    var picker = __instance.positionPickers[i];
                    var storage = picker.EnsureComponent<PickerStorage>();

                    if (!__instance.EqualValueForSelectedEvents(i, PropertyControlUtil.Component.X))
                    {
                        picker.x.text = string.Empty;
                        storage.lastX = string.Empty;
                        ((Text)picker.x.placeholder).text = InspectorUtil.MixedTextShorter;
                        __runOriginal = false;
                    }
                    else
                    {
                        ((Text)picker.x.placeholder).text = InspectorUtil.DefaultNullText;
                    }

                    if (!__instance.EqualValueForSelectedEvents(i, PropertyControlUtil.Component.Y))
                    {
                        picker.y.text = string.Empty;
                        storage.lastY = string.Empty;
                        ((Text)picker.y.placeholder).text = InspectorUtil.MixedTextShorter;
                        __runOriginal = false;
                    }
                    else
                    {
                        ((Text)picker.y.placeholder).text = InspectorUtil.DefaultNullText;
                    }

                    if (__runOriginal)
                    {
                        storage.lastX = picker.x.text;
                        storage.lastY = picker.y.text;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PropertyControl_SetRoomPerspective), nameof(PropertyControl_SetRoomPerspective.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_SetRoomPerspective __instance, InspectorPanel.ChangeAction action)
            {
                var updateStorage = __instance.EnsureComponent<PropertyUpdateStorage>();

                for (int i = 0; i < __instance.positionPickers.Length; i++)
                {
                    var picker = __instance.positionPickers[i];
                    var storage = picker.EnsureComponent<PickerStorage>();

                    picker.x.onEndEdit.RemoveAllListeners();
                    picker.y.onEndEdit.RemoveAllListeners();

                    picker.x.onEndEdit.AddListener(text =>
                    {
                        string sound = picker.PlayOnEndEdit ? "sndEditorValueChange" : null;

                        if (storage.lastX == text)
                        {
                            if (sound != null)
                            {
                                __instance.inspectorPanel.LevelEditorPlaySound(sound, __instance.inspectorPanel.defaultGroup);
                            }

                            return;
                        }

                        ((Text)picker.x.placeholder).text = InspectorUtil.DefaultNullText;
                        storage.lastX = text;
                        updateStorage.type = PropertyUpdateType.UpdateX;
                        action(null, sound, null);
                    });

                    picker.y.onEndEdit.AddListener(text =>
                    {
                        string sound = picker.PlayOnEndEdit ? "sndEditorValueChange" : null;

                        if (storage.lastY == text)
                        {
                            if (sound != null)
                            {
                                __instance.inspectorPanel.LevelEditorPlaySound(sound, __instance.inspectorPanel.defaultGroup);
                            }

                            return;
                        }

                        ((Text)picker.y.placeholder).text = InspectorUtil.DefaultNullText;
                        storage.lastY = text;
                        updateStorage.type = PropertyUpdateType.UpdateY;
                        action(null, sound, null);
                    });

                    picker.units.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        storage.lastX = picker.x.text;
                        storage.lastY = picker.y.text;
                    });
                }
            }
        }

        [HarmonyPatch(typeof(PropertyControl_SetRoomPerspective), nameof(PropertyControl_SetRoomPerspective.Save))]
        private static class Save
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStelemAny<Float2>())
                    .Emit(OpCodes.Ldloc_2)
                    .Emit(OpCodes.Ldarg_0)
                    .Emit(OpCodes.Ldarg_1)
                    .EmitDelegate((Float2 float2, int i, PropertyControl_SetRoomPerspective propertyControl, LevelEvent_Base levelEvent) =>
                    {
                        PropertyUpdateType type = GetUpdateType(propertyControl);
                        bool applyX = type.HasFlag(PropertyUpdateType.UpdateX);
                        bool applyY = type.HasFlag(PropertyUpdateType.UpdateY);

                        if (applyX && applyY)
                        {
                            return float2;
                        }

                        Float2[] currentValue = (Float2[])propertyControl.GetEventValue(levelEvent);

                        if (applyX && !applyY)
                        {
                            return Float2Util.Combine(float2, currentValue[i]);
                        }

                        return Float2Util.Combine(currentValue[i], float2);
                    });
            }
        }

        private static PropertyUpdateType GetUpdateType(PropertyControl property)
        {
            return property.TryGetComponent(out PropertyUpdateStorage storage) ? storage.type : PropertyUpdateType.UpdateBoth;
        }

        private class PropertyUpdateStorage : MonoBehaviour
        {
            public PropertyUpdateType type = PropertyUpdateType.UpdateBoth;
        }

        private class PickerStorage : MonoBehaviour
        {
            public string lastX = null;
            public string lastY = null;
        }

        [Flags]
        private enum PropertyUpdateType
        {
            UpdateX = 1,
            UpdateY = 2,
            UpdateBoth = UpdateX | UpdateY
        }
    }
}
