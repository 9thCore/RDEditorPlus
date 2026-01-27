using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_ToggleGroup
    {
        [HarmonyPatch(typeof(PropertyControl_ToggleGroup), nameof(PropertyControl_ToggleGroup.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_ToggleGroup __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.toggleGroup.SetAllTogglesOff(sendCallback: false);
                    return false;
                }

                return true;
            }
        }
    }
}
