using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.SubRow;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_TabSection_Rows
    {
        [HarmonyPatch(typeof(TabSection_Rows), nameof(TabSection_Rows.Setup))]
        private static class Setup
        {
            private static void Postfix(TabSection_Rows __instance)
            {
                if (!PluginConfig.PatientSubRowsEnabled)
                {
                    return;
                }

                SubRowStorage.Instance.SetupWithScrollMaskIntermediary(__instance.rowsListRect, "Rows");
                __instance.rowsListRect.offsetMin = Vector2.zero;
                __instance.rowsListRect.offsetMax = Vector2.zero;

                __instance.rowsListRect.EnsureComponent<RowTabScroller>();
            }
        }
    }
}
