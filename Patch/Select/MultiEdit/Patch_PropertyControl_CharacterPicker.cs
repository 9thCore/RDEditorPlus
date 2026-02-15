using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_CharacterPicker
    {
        [HarmonyPatch(typeof(PropertyControl_CharacterPicker), nameof(PropertyControl_CharacterPicker.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_CharacterPicker __instance)
            {
                InspectorUtil.SetupMixedPlaceholder(__instance.customInput);
            }
        }

        [HarmonyPatch(typeof(PropertyControl_CharacterPicker), nameof(PropertyControl_CharacterPicker.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_CharacterPicker __instance)
            {
                if (!__instance.characterPicker.returnsExpression
                    || !InspectorUtil.CanMultiEdit())
                {
                    SetInteractable(__instance, true);
                    ((Text)__instance.customInput.placeholder).text = string.Empty;
                    return true;
                }

                SetInteractable(__instance, PropertyControlUtil.EqualCharacterForSelectedEvents());

                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.characterPicker.label.text = InspectorUtil.MixedText;
                    __instance.customInput.text = string.Empty;
                    ((Text)__instance.customInput.placeholder).text = InspectorUtil.MixedText;
                    return false;
                }

                ((Text)__instance.customInput.placeholder).text = string.Empty;
                return true;
            }

            private static void SetInteractable(PropertyControl_CharacterPicker picker, bool interactable)
            {
                picker.characterPicker.GetComponent<Button>().interactable = interactable;
                picker.customInput.interactable = interactable;
                picker.switchDropdown.interactable = interactable;
            }
        }
    }
}
