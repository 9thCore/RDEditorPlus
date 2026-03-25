using HarmonyLib;
using RDEditorPlus.Functionality.LevelNode;
using RDLevelEditor;

namespace RDEditorPlus.Patch.LevelNode
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Update))]
        private static class Update
        {
            private static bool Prefix()
            {
                if (!scnEditor.instance.userIsEditingAnInputField
                    && RDEditorUtils.CheckForKeyCombo(false, false, PluginConfig.LevelNodeKeyCode))
                {
                    LevelNodePanelHolder.Instance.Toggle();
                    return false;
                }

                return true;
            }
        }
    }
}
