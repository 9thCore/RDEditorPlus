using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_InputField
    {
        [HarmonyPatch(typeof(PropertyControl_InputField), nameof(PropertyControl_InputField.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_InputField __instance)
            {
                RectTransform template = (RectTransform) __instance.inputField.textComponent.transform;

                GameObject instance = Object.Instantiate(template.gameObject);
                instance.SetActive(false);

                RectTransform transform = (RectTransform) instance.transform;

                transform.SetParent(__instance.inputField.transform);
                transform.localRotation = template.localRotation;
                transform.localScale = template.localScale;
                transform.offsetMin = template.offsetMin;
                transform.offsetMax = template.offsetMax;

                Text text = instance.GetComponent<Text>();
                InspectorUtil.SetupMixedText(text);
                __instance.inputField.placeholder = text;

                instance.SetActive(true);
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
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_InputField), nameof(PropertyControl_InputField.Save))]
        private static class Save
        {
            private static bool Prefix(PropertyControl_InputField __instance)
            {
                if (string.IsNullOrEmpty(__instance.inputField.text))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
