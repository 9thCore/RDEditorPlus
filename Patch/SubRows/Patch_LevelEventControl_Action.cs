using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_LevelEventControl_Action
    {
        [HarmonyPatch(typeof(LevelEventControl_Action), nameof(LevelEventControl_Action.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_Action __instance)
            {
                switch (__instance.levelEvent.GetTab())
                {
                    case Tab.Song:
                    case Tab.Actions:
                        return;
                }

                GeneralManager.Instance.UpdateUI(__instance);
            }
        }
    }
}
