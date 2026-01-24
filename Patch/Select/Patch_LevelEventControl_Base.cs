using HarmonyLib;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select
{
    internal static class Patch_LevelEventControl_Base
    {
        [HarmonyPatch(typeof(LevelEventControl_Base), nameof(LevelEventControl_Base.SaveData))]
        private static class SaveData
        {
            private static bool Prefix(LevelEventControl_Base __instance)
            {
                if (!PluginConfig.SelectionMultiEditEnabled)
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
