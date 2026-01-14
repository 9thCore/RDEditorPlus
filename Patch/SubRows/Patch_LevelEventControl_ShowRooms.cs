using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_LevelEventControl_ShowRooms
    {
        [HarmonyPatch(typeof(LevelEventControl_ShowRooms), nameof(LevelEventControl_ShowRooms.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_ShowRooms __instance)
            {
                RoomManager.Instance.UpdateFullTimelineHeightEvent(__instance);
            }
        }
    }
}
