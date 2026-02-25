using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.SubRow;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.SubRows.Patient
{
    internal static class Patch_TabSection_Rows
    {
        [HarmonyPatch(typeof(TabSection_Rows), nameof(TabSection_Rows.Setup))]
        [HarmonyAfter(Plugin.RDModificationsGUID)]
        private static class Setup
        {
            private static void Postfix(TabSection_Rows __instance)
            {
                if (__instance.rowsListRect.transform.parent.name.Contains(Plugin.RDModificationsMaskName))
                {
                    Plugin.RDModificationsRowPatchEnabled = true;
                }
                else
                {
                    SubRowStorage.Instance.SetupWithScrollMaskIntermediary(__instance.rowsListRect, "Rows");

                    __instance.rowsListRect.offsetMin = Vector2.zero;
                    __instance.rowsListRect.offsetMax = Vector2.zero;
                }

                __instance.rowsListRect.EnsureComponent<RowTabScroller>();
            }
        }
    }
}
