using HarmonyLib;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;

namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal static class Patch_Timeline
    {
        [HarmonyPatch(typeof(Timeline), $"get_{nameof(Timeline.usedRowCount)}")]
        private static class get_usedRowCount
        {
            private static void Postfix(Timeline __instance, ref int __result)
            {
                if (ShouldApply)
                {
                    if (PluginConfig.SubRowsEnabled && PluginConfig.WindowSubRowsEnabled)
                    {
                        __result++;
                    }
                    else
                    {
                        __result += Math.Max(0, MoreWindowManager.Instance.WindowCount - __instance.rowCellCount + 1);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.UpdateUIInternalCo), MethodType.Enumerator)]
        private static class UpdateUIInternalCo
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchStfld<Timeline>(nameof(Timeline.isUpdatingUI)))
                    .EmitDelegate(() =>
                    {
                        var timeline = scnEditor.instance.timeline;

                        var rect = scnEditor.instance.tabSection_windows.listRect;
                        rect.OffsetMinY(rect.offsetMin.y - timeline.cellHeight * (1 + MoreWindowManager.Instance.ExtraWindowCount));
                    });
            }
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.LateUpdate))]
        private static class LateUpdate
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStloc(2))
                    .EmitDelegate((int rowCount) =>
                    {
                        if (ShouldApply)
                        {
                            if (PluginConfig.SubRowsEnabled && PluginConfig.WindowSubRowsEnabled)
                            {
                                rowCount -= Math.Max(0, scnEditor.instance.timeline.maxUsedY - scnEditor.instance.timeline.rowCellCount) + 1;
                            }
                            else
                            {
                                rowCount += MoreWindowManager.Instance.ExtraWindowCount;
                                rowCount = Math.Min(rowCount, scnEditor.instance.timeline.scaledRowCellCount - 1);
                            }
                        }

                        return rowCount;
                    });
            }
        }

        private static bool ShouldApply => scnEditor.instance.currentTab == Tab.Windows;
    }
}
