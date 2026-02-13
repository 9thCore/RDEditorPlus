using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_Image
    {
        [HarmonyPatch(typeof(PropertyControl_Image), nameof(PropertyControl_Image.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_Image __instance, InspectorPanel.ChangeAction action)
            {
                InspectorUtil.SetupMixedPlaceholder(__instance.filename);

                __instance.filename.onValueChanged.AddListener(text =>
                {
                    if (__instance.EqualValueForSelectedEvents())
                    {
                        ResetPlaceholder(__instance);
                    }
                });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Image), nameof(PropertyControl_Image.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_Image __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.filename.text = string.Empty;
                    ((Text)__instance.filename.placeholder).text = InspectorUtil.MixedText;
                    return false;
                }

                ResetPlaceholder(__instance);
                return true;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Image), nameof(PropertyControl_Image.Save))]
        private static class Save
        {
            private static bool Prefix(PropertyControl_Image __instance)
            {
                if (__instance.filename.text.IsNullOrEmpty()
                    && ((Text)__instance.filename.placeholder).text == InspectorUtil.MixedText)
                {
                    return false;
                }

                return true;
            }
        }

        private static void ResetPlaceholder(PropertyControl_Image propertyControl)
        {
            ((Text)propertyControl.filename.placeholder).text = RDString.Get("editor.noImage");
        }
    }
}
