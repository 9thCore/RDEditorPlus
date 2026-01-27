using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_InputField
    {
        [HarmonyPatch(typeof(PropertyControl_InputField), nameof(PropertyControl_InputField.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_InputField __instance, InspectorPanel.ChangeAction action)
            {
                InspectorUtil.SetupMixedPlaceholder(__instance.inputField);

                RDInputField inputField = (RDInputField)__instance.inputField;

                if (__instance.AcceptsNull())
                {
                    __instance.inputField.onValueChanged.AddListener(text =>
                    {
                        ResetPlaceholder(__instance, text);
                    });
                }

                __instance.inputField.onEndEdit.AddListener(text =>
                {
                    inputField.selected = false;
                    ResetPlaceholder(__instance, text);
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_InputField), nameof(PropertyControl_InputField.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_InputField __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.inputField.text = string.Empty;
                    ((Text)__instance.inputField.placeholder).text = InspectorUtil.MixedText;
                    return false;
                }

                ResetPlaceholder(__instance, __instance.inputField.text);
                return true;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_InputField), nameof(PropertyControl_InputField.Save))]
        private static class Save
        {
            private static bool Prefix(PropertyControl_InputField __instance)
            {
                if (__instance.inputField.text.IsNullOrEmpty()
                    && (!__instance.AcceptsNull() || ((Text)__instance.inputField.placeholder).text == InspectorUtil.MixedText))
                {
                    return false;
                }

                return true;
            }
        }

        private static void ResetPlaceholder(PropertyControl_InputField propertyControl, string text)
        {
            if (propertyControl.AcceptsNull())
            {
                ((Text)propertyControl.inputField.placeholder).text = InspectorUtil.DefaultNullText;
            }
            else
            {
                ((Text)propertyControl.inputField.placeholder).text = string.Empty;
            }
        }
    }
}
