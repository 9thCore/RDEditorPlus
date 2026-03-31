using HarmonyLib;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.RecullOnlyIfRequired
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DecodeData))]
        private static class DecodeData
        {
            private static void Postfix(scnEditor __instance)
            {
                foreach (var control in __instance.eventControls)
                {
                    control.UpdateUIInternal();
                }
            }
        }
    }
}
