using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_Slider
    {
        [HarmonyPatch(typeof(PropertyControl_Slider), nameof(PropertyControl_Slider.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_Slider __instance)
            {
                InspectorUtil.SetupMixedPlaceholder(__instance.inputField, InspectorUtil.MixedTextSliderPercent);
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Slider), nameof(PropertyControl_Slider.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_Slider __instance)
            {
                RDEventTrigger trigger = __instance.slider.GetComponent<RDEventTrigger>();
                trigger.onClick = (RDPointerEventData) Delegate.Combine(new RDPointerEventData((PointerEventData _) =>
                {
                    __instance.slider.handleRect.GetComponent<Graphic>().SetAlpha(1f);
                    __instance.slider.fillRect.gameObject.SetActive(true);
                    __instance.inputField.text = __instance.slider.value.ToString();
                }), trigger.onClick);

                __instance.inputField.onEndEdit.AddListener(_ =>
                {
                    __instance.slider.handleRect.GetComponent<Graphic>().SetAlpha(1f);
                    __instance.slider.fillRect.gameObject.SetActive(true);
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Slider), nameof(PropertyControl_Slider.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_Slider __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.inputField.text = string.Empty;
                    __instance.slider.SetNormalizedValueWithoutNotify(0.5f);
                    __instance.slider.handleRect.GetComponent<Graphic>().SetAlpha(InspectorUtil.MixedSliderAlpha);
                    __instance.slider.fillRect.gameObject.SetActive(false);
                    return false;
                }

                __instance.slider.handleRect.GetComponent<Graphic>().SetAlpha(1f);
                __instance.slider.fillRect.gameObject.SetActive(true);
                return true;
            }
        }
    }
}
