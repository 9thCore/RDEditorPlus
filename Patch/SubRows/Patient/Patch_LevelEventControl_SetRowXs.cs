using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows.Patient
{
    internal static class Patch_LevelEventControl_SetRowXs
    {
        [HarmonyPatch(typeof(LevelEventControl_SetRowXs), nameof(LevelEventControl_SetRowXs.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_SetRowXs __instance)
            {
                RowManager.Instance.UpdateUI(__instance);
            }
        }
    }
}
