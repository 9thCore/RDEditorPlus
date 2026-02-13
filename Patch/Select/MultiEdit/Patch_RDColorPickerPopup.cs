using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_RDColorPickerPopup
    {
        [HarmonyPatch(typeof(RDColorPickerPopup), nameof(RDColorPickerPopup.Start))]
        private static class Start
        {
            private static void Postfix(RDColorPickerPopup __instance)
            {
                __instance.rInput.onEndEdit.AddListener(OnEndEdit);
                __instance.gInput.onEndEdit.AddListener(OnEndEdit);
                __instance.bInput.onEndEdit.AddListener(OnEndEdit);
                __instance.aInput.onEndEdit.AddListener(OnEndEdit);
            }

            private static void OnEndEdit(string _)
            {
                PropertyStorage.Instance.colorChanged = true;
                SetAlpha(scnEditor.instance.colorPickerPopup, fullOpacity: true);
            }
        }

        [HarmonyPatch(typeof(RDColorPickerPopup), nameof(RDColorPickerPopup.Show))]
        private static class Show
        {
            private static void Postfix(RDColorPickerPopup __instance)
            {
                lastColor = __instance.color;
                PropertyStorage.Instance.colorChanged = false;
                SetAlpha(__instance, PropertyStorage.Instance.colorPropertyEqual);
            }
        }

        [HarmonyPatch(typeof(RDColorPickerPopup), nameof(RDColorPickerPopup.Update))]
        private static class Update
        {
            private static void Postfix(RDColorPickerPopup __instance)
            {
                if (PropertyStorage.Instance.colorChanged)
                {
                    return;
                }

                if (lastColor != __instance.color
                    || (Input.GetMouseButtonDown(0) && CUIColorPicker.GetLocalMouse(__instance.cuiColorPicker.satvalGO, out _)))
                {
                    PropertyStorage.Instance.colorChanged = true;
                    SetAlpha(__instance, fullOpacity: true);
                }
            }
        }

        private static void SetAlpha(RDColorPickerPopup popup, bool fullOpacity)
        {
            popup.cuiColorPicker.satvalKnob.GetComponent<Image>()
                .color = Color.white.WithAlpha(fullOpacity ? 1.0f : InspectorUtil.MixedColorKnobAlpha);
        }

        private static string lastColor = string.Empty;
    }
}
