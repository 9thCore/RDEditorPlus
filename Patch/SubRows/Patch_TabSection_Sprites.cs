using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_TabSection_Sprites
    {
        [HarmonyPatch(typeof(TabSection_Sprites), nameof(TabSection_Sprites.UpdateHeadersToPage))]
        private static class UpdateHeadersToPage
        {
            private static void Postfix()
            {
                SpriteManager.Instance.UpdateTab(force: true);
                scnEditor.instance.timeline.UpdateMaxUsedY();
            }
        }
    }
}
