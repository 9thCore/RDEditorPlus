using HarmonyLib;
using RDEditorPlus.Functionality.NodeEditor;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.LevelMerger
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
                    MergerPanelHolder.Instance.Toggle(true);
                }
            }
        }
    }
}
