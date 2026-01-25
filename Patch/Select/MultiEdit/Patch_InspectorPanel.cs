using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_InspectorPanel
    {
        [HarmonyPatch(typeof(InspectorPanel), nameof(InspectorPanel.Show))]
        private static class Show
        {
            private static void Prefix()
            {
                PropertyStorage.Instance.UnmarkAll();
            }
        }
    }
}
