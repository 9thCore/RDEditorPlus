using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_TabSection
    {
        [HarmonyPatch(typeof(TabSection), nameof(TabSection.ShowMultipleSelectedPanel))]
        private static class ShowMultipleSelectedPanel
        {
            private static bool Prefix()
            {
                return !InspectorUtil.CanMultiEdit();
            }
        }
    }
}
