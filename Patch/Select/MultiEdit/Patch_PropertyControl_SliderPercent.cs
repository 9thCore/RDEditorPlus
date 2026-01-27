using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_SliderPercent
    {
        [HarmonyPatch(typeof(PropertyControl_SliderPercent), nameof(PropertyControl_SliderPercent.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_SliderPercent __instance)
            {
                InspectorUtil.SetupMixedPlaceholder(__instance.inputField, InspectorUtil.MixedTextSliderPercent);
            }
        }

        [HarmonyPatch(typeof(PropertyControl_SliderPercent), nameof(PropertyControl_SliderPercent.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_SliderPercent __instance)
            {
                __instance.slider.onValueChanged.AddListener(_ =>
                {
                    __instance.slider.handleRect.parent.gameObject.SetActive(true);
                    __instance.slider.fillRect.parent.gameObject.SetActive(true);
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_SliderPercent), nameof(PropertyControl_SliderPercent.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_SliderPercent __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.inputField.text = string.Empty;
                    __instance.slider.handleRect.parent.gameObject.SetActive(false);
                    __instance.slider.fillRect.parent.gameObject.SetActive(false);
                    return false;
                }

                return true;
            }
        }
    }
}
