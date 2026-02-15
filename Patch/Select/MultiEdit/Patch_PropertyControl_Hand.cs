using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_Hand
    {
        [HarmonyPatch(typeof(PropertyControl_Hand), nameof(PropertyControl_Hand.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_Hand __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.hand.SetAllTogglesOff(sendCallback: false);
                    return false;
                }

                return true;
            }
        }
    }
}
