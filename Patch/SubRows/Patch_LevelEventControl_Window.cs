using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_LevelEventControl_Window
    {
        [HarmonyPatch(typeof(LevelEventControl_Window), nameof(LevelEventControl_Window.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_Window __instance)
            {
                if (!PluginConfig.WindowSubRowsEnabled)
                {
                    return;
                }

                if (__instance.levelEvent.type == LevelEventType.ReorderWindows
                    || __instance.levelEvent.type == LevelEventType.DesktopColor)
                {
                    WindowManager.Instance.UpdateFullTimelineHeightEvent(__instance);
                } else
                {
                    WindowManager.Instance.UpdateUI(__instance);
                }
            }
        }
    }
}
