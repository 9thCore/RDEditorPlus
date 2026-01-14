using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_LevelEventControl_Room
    {
        [HarmonyPatch(typeof(LevelEventControl_Room), nameof(LevelEventControl_Room.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_Room __instance)
            {
                RoomManager.Instance.UpdateUI(__instance);
            }
        }
    }
}
