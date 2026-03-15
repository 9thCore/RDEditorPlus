using HarmonyLib;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions;
using RDEditorPlus.Util;
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
    }
}
