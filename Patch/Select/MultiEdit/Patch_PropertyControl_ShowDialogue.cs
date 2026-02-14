using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_ShowDialogue
    {
        [HarmonyPatch(typeof(PropertyControl_ShowDialogue), nameof(PropertyControl_ShowDialogue.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_ShowDialogue __instance, InspectorPanel.ChangeAction action)
            {
                InspectorUtil.SetupMixedPlaceholder(__instance.inputField);

                RDInputField inputField = (RDInputField)__instance.inputField;

                __instance.inputField.onValueChanged.AddListener(text =>
                {
                    if (__instance.EqualValueForSelectedEvents())
                    {
                        ResetPlaceholder(__instance);
                    }
                });

                __instance.inputField.onEndEdit.AddListener(text =>
                {
                    inputField.selected = false;

                    if (__instance.EqualValueForSelectedEvents())
                    {
                        ResetPlaceholder(__instance);
                    }
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_ShowDialogue), nameof(PropertyControl_ShowDialogue.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_ShowDialogue __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.ResetCaretPosition();
                    __instance.inputField.text = string.Empty;
                    ((Text)__instance.inputField.placeholder).text = InspectorUtil.MixedText;
                    return false;
                }

                ResetPlaceholder(__instance);
                return true;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_ShowDialogue), nameof(PropertyControl_ShowDialogue.Save))]
        private static class Save
        {
            private static bool Prefix(PropertyControl_ShowDialogue __instance)
            {
                if (__instance.inputField.text.IsNullOrEmpty()
                    && ((Text)__instance.inputField.placeholder).text == InspectorUtil.MixedText)
                {
                    return false;
                }

                return true;
            }
        }

        private static void ResetPlaceholder(PropertyControl_ShowDialogue propertyControl)
        {
            ((Text)propertyControl.inputField.placeholder).text = string.Empty;
        }
    }
}
