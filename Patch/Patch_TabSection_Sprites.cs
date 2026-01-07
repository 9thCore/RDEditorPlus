using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;

namespace RDEditorPlus.Patch
{
    [HarmonyPatch]
    internal static class Patch_TabSection_Sprites
    {
        [HarmonyPatch(typeof(TabSection_Sprites), nameof(TabSection_Sprites.UpdateHeadersToPage))]
        private static class UpdateHeadersToPage
        {
            private static void Postfix(TabSection_Sprites __instance)
            {
                SubRowStorage.Holder.UpdateSpriteHeaders(__instance);
            }
        }
    }
}
