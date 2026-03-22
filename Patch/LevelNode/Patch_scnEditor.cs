using HarmonyLib;
using RDEditorPlus.Functionality.LevelNode;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.LevelNode
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Update))]
        private static class Update
        {
            private static void Postfix()
            {
                if (RDEditorUtils.CheckForKeyCombo(false, false, KeyCode.R))
                {
                    LevelNodePanelHolder.Instance.Toggle(true);
                }
            }
        }
    }
}
