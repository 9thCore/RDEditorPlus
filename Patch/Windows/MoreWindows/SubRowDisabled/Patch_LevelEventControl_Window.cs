using HarmonyLib;
using RDEditorPlus.Functionality.Windows;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.Windows.MoreWindows.SubRowDisabled
{
    internal static class Patch_LevelEventControl_Window
    {
        [HarmonyPatch(typeof(LevelEventControl_Window), nameof(LevelEventControl_Window.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_Window __instance)
            {
                switch (__instance.levelEvent.type)
                {
                    case LevelEventType.ReorderWindows:
                    case LevelEventType.DesktopColor:
                        var timeline = scnEditor.instance.timeline;
                        var rt = __instance.rt;

                        rt.sizeDelta = new Vector2(timeline.cellWidth, timeline.cellHeight * MoreWindowManager.Instance.WindowCount);
                        break;
                }
            }
        }
    }
}
