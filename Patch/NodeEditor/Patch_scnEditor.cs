using HarmonyLib;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor;
using RDLevelEditor;

namespace RDEditorPlus.Patch.NodeEditor
{
    internal class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Start))]
        private static class Start
        {
            private static void Postfix()
            {
                NodeLibrary.Instance.Prime();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Update))]
        private static class Update
        {
            private static bool Prefix()
            {
                if (NodePanelHolder.CurrentPanel == null)
                {
                    return true;
                }

                NodePanelHolder.CurrentPanel.OnUpdate();
                return false;
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.StartPlaying))]
        private static class StartPlaying
        {
            private static bool Prefix() => NodePanelHolder.CurrentPanel == null;
        }
    }
}
