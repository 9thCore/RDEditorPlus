using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select
{
    internal static class Patch_RDInspectorPanelManager
    {
        [HarmonyPatch(typeof(RDInspectorPanelManager), nameof(RDInspectorPanelManager.ShowBulkPanel))]
        private static class ShowBulkPanel
        {
            private static bool Prefix()
            {
                if (!InspectorUtil.CanMultiEdit())
                {
                    return true;
                }

                scnEditor.instance.selectedControls[0].levelEvent.inspectorPanel.position.UpdateUI();
                return false;
            }
        }
    }
}
