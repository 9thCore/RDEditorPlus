using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_LevelEventControl_Base
    {
        [HarmonyPatch(typeof(LevelEventControl_Base), nameof(LevelEventControl_Base.SaveData))]
        private static class SaveData
        {
            private static bool Prefix(LevelEventControl_Base __instance)
            {
                if (!InspectorUtil.CanMultiEdit())
                {
                    return true;
                }

                if (scnEditor.instance.changingState == 0)
                {
                    __instance.levelEvent.SaveData();
                }

                return false;
            }
        }
    }
}
