using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_TabSection
    {
        [HarmonyPatch(typeof(TabSection), nameof(TabSection.ChangePage))]
        private static class ChangePage
        {
            private static void Postfix()
            {
                if (scnEditor.instance.currentTab == Tab.Rows
                    && PluginConfig.PatientSubRowsEnabled)
                {
                    RowManager.Instance.UpdateTab(force: false);
                    scnEditor.instance.timeline.UpdateMaxUsedY();
                }
            }
        }
    }
}
