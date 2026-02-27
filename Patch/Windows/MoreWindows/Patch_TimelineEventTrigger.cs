using HarmonyLib;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal static class Patch_TimelineEventTrigger
    {
        [HarmonyPatch(typeof(TimelineEventTrigger), nameof(TimelineEventTrigger.OnPointerClick))]
        private static class OnPointerClick
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(instruction => instruction.MatchLdloc(8), instruction => instruction.MatchLdcI4((int)Tab.Windows))
                    .GotoNext(MoveType.After, instruction => instruction.MatchLdcI4(RDEditorConstants.WindowCount))
                    .EmitDelegate((int _) => MoreWindowManager.Instance.WindowCount);
            }
        }
    }
}
