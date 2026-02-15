using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_SliderAlpha
    {
        [HarmonyPatch(typeof(PropertyControl_SliderAlpha), nameof(PropertyControl_SliderAlpha.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_SliderAlpha __instance)
            {
                InspectorUtil.SetupMixedPlaceholder(__instance.inputField, InspectorUtil.MixedTextSliderPercent);
            }
        }

        [HarmonyPatch(typeof(PropertyControl_SliderAlpha), nameof(PropertyControl_SliderAlpha.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_SliderAlpha __instance)
            {
                RDEventTrigger trigger = __instance.slider.GetComponent<RDEventTrigger>();
                trigger.onClick = (RDPointerEventData) Delegate.Combine(new RDPointerEventData((PointerEventData _) =>
                {
                    __instance.slider.handleRect.GetComponent<Graphic>().SetAlpha(1f);
                    __instance.slider.value = __instance.slider.value;
                }), trigger.onClick);

                __instance.inputField.onEndEdit.AddListener(_ =>
                {
                    __instance.slider.handleRect.GetComponent<Graphic>().SetAlpha(1f);
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_SliderAlpha), nameof(PropertyControl_SliderAlpha.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_SliderAlpha __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.inputField.text = string.Empty;
                    __instance.slider.SetNormalizedValueWithoutNotify(0.5f);
                    __instance.slider.handleRect.GetComponent<Graphic>().SetAlpha(InspectorUtil.MixedSliderAlpha);
                    return false;
                }

                __instance.slider.handleRect.GetComponent<Graphic>().SetAlpha(1f);
                return true;
            }
        }
    }
}
