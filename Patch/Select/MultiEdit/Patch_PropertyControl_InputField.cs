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

                __instance.inputField.onEndEdit.AddListener(_ =>
                {
                    inputField.selected = false;
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
