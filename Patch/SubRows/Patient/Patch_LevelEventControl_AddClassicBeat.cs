using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows.Patient
{
    internal static class Patch_LevelEventControl_AddClassicBeat
    {
        [HarmonyPatch(typeof(LevelEventControl_AddClassicBeat), nameof(LevelEventControl_AddClassicBeat.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_AddClassicBeat __instance)
            {
                RowManager.Instance.UpdateUI(__instance);
            }
        }
    }
}
