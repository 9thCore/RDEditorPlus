using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_LevelEventControl_RowAction
    {
        [HarmonyPatch(typeof(LevelEventControl_RowAction), nameof(LevelEventControl_RowAction.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_RowAction __instance)
            {
                if (!PluginConfig.PatientSubRowsEnabled)
                {
                    return;
                }

                RowManager.Instance.UpdateUI(__instance);
            }
        }
    }
}
