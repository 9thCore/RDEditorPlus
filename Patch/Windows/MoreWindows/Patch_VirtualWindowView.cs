using HarmonyLib;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal static class Patch_VirtualWindowView
    {
        [HarmonyPatch(typeof(VirtualWindowView), nameof(VirtualWindowView.Setup))]
        private static class Setup
        {
            private static void Postfix(VirtualWindowView __instance)
            {
                __instance.index %= RDEditorConstants.windowColors.Length;
            }
        }
    }
}