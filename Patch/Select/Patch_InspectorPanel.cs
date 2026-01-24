using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select
{
    internal static class Patch_InspectorPanel
    {
        [HarmonyPatch(typeof(InspectorPanel), nameof(InspectorPanel.Show))]
        private static class Show
        {
            private static void Prefix(InspectorPanel __instance)
            {
                PropertyStorage.Instance.UnmarkAll();
            }
        }
    }
}
