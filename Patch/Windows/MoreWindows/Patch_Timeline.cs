using HarmonyLib;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal static class Patch_Timeline
    {
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
                        var rect = scnEditor.instance.tabSection_windows.listRect;
                        rect.OffsetMinY(rect.offsetMin.y - scnEditor.instance.cellHeight * (1 + MoreWindowManager.Instance.ExtraWindowCount));
                    });
            }
        }
    }
}
