using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
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
                if (__instance.levelEvent.type != LevelEventType.Comment)
                {
                    return;
                }

                GeneralManager.Instance.UpdateUI(__instance);
            }
        }
    }
}
