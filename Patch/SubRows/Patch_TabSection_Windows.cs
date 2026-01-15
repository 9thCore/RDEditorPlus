using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.SubRows
{
    internal class Patch_TabSection_Windows
    {
        [HarmonyPatch(typeof(TabSection_Windows), nameof(TabSection_Windows.Setup))]
        private static class Setup
        {
            private static void Postfix(TabSection_Windows __instance)
            {
                if (!PluginConfig.WindowSubRowsEnabled)
                {
                    return;
                }

                SubRowStorage.Instance.SetupWithScrollMaskIntermediary(__instance.listRect, "Windows");
                __instance.listRect.offsetMin = Vector2.zero;
                __instance.listRect.offsetMax = Vector2.zero;
            }
        }

        [HarmonyPatch(typeof(TabSection_Windows), nameof(TabSection_Windows.Update))]
        private static class Update
        {
            private static void Postfix(TabSection_Windows __instance)
            {
                if (!PluginConfig.WindowSubRowsEnabled)
                {
                    return;
                }

                WindowManager.Instance.UpdateTabScroll();
            }
        }
    }
}
