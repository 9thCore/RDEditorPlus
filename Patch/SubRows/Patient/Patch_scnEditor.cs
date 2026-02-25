using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows.Patient
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.AddNewRow))]
        private static class AddNewRow
        {
            private static void Postfix()
            {
                RowManager.Instance.UpdateTab(force: false);
                scnEditor.instance.timeline.UpdateMaxUsedY();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.MoveRowVertically))]
        private static class MoveRowVertically
        {
            private static void Postfix()
            {
                RowManager.Instance.UpdateTab(force: false);
            }
        }
    }
}
