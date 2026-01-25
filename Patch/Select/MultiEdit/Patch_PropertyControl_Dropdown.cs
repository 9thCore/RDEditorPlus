using HarmonyLib;
using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_Dropdown
    {
        [HarmonyPatch(typeof(PropertyControl_Dropdown), nameof(PropertyControl_Dropdown.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_Dropdown __instance)
            {
                CustomDropdown customDropdown = __instance.dropdown.ReplaceWithDerivative<CustomDropdown>();
                __instance.dropdown = customDropdown;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Dropdown), nameof(PropertyControl_Dropdown.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_Dropdown __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.dropdown.captionText.text = InspectorUtil.MixedText;
                    return false;
                }

                return true;
            }
        }
    }
}
