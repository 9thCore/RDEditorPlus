using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_Checkbox
    {
        [HarmonyPatch(typeof(PropertyControl_Checkbox), nameof(PropertyControl_Checkbox.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_Checkbox __instance)
            {
                Graphic graphic = __instance.checkbox.graphic;

                Graphic mixedGraphic = Object.Instantiate(graphic.gameObject).GetComponent<Graphic>();
                mixedGraphic.gameObject.SetActive(false);

                mixedGraphic.transform.SetParent(graphic.transform.parent);
                mixedGraphic.transform.localScale = graphic.transform.localScale;
                mixedGraphic.rectTransform.offsetMin = graphic.rectTransform.offsetMin;
                mixedGraphic.rectTransform.offsetMax = graphic.rectTransform.offsetMax;
                mixedGraphic.rectTransform.anchoredPosition = graphic.rectTransform.anchoredPosition;
                mixedGraphic.SetAlpha(InspectorUtil.MixedCheckboxAlpha);

                MixedGraphic[__instance] = mixedGraphic;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Checkbox), nameof(PropertyControl_Checkbox.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_Checkbox __instance)
            {
                __instance.checkbox.onValueChanged.AddListener(_ =>
                {
                    MixedGraphic[__instance].gameObject.SetActive(false);
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Checkbox), nameof(PropertyControl_Checkbox.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_Checkbox __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.checkbox.SetIsOnWithoutNotify(false);
                    MixedGraphic[__instance].gameObject.SetActive(true);
                    return false;
                }

                MixedGraphic[__instance].gameObject.SetActive(false);
                return true;
            }
        }

        private static readonly Dictionary<PropertyControl, Graphic> MixedGraphic = new();
    }
}
