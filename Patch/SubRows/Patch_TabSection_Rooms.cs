using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_TabSection_Rooms
    {
        [HarmonyPatch(typeof(TabSection_Rooms), nameof(TabSection_Rooms.Setup))]
        private static class Setup
        {
            private static void Postfix(TabSection_Rooms __instance)
            {
                if (!PluginConfig.RoomSubRowsEnabled)
                {
                    return;
                }

                SubRowStorage.Instance.SetupWithScrollMaskIntermediary(__instance.listRect, "Rooms");
                __instance.listRect.offsetMin = Vector2.zero;
                __instance.listRect.offsetMax = Vector2.zero;
            }
        }

        [HarmonyPatch(typeof(TabSection_Rooms), nameof(TabSection_Rooms.Update))]
        private static class Update
        {
            private static void Postfix(TabSection_Rooms __instance)
            {
                RoomManager.Instance.UpdateTabScroll();
            }
        }
    }
}
