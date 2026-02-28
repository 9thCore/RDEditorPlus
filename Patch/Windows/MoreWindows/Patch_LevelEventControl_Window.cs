using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal static class Patch_LevelEventControl_Window
    {
        [HarmonyPatch(typeof(LevelEventControl_Window), nameof(LevelEventControl_Window.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                ILLabel label = null;

                cursor
                    .GotoNext(instruction => instruction.MatchIsinst<LevelEvent_ReorderWindows>())
                    .GotoNext(instruction => instruction.MatchBrfalse(out label))
                    .GotoNext(MoveType.Before, instruction => instruction.MatchLdfld<LevelEventControl_Window>(nameof(LevelEventControl_Window.roomIcons)));

                cursor
                    .EmitDelegate((LevelEventControl_Window eventControl) =>
                    {
                        if (PluginConfig.SubRowsEnabled && PluginConfig.WindowSubRowsEnabled
                        && PluginConfig.TallEventSubRowsBehaviour != PluginConfig.SubRowTallEventBehaviour.ExpandToTimelineHeight)
                        {
                            return false;
                        }

                        eventControl.icon.gameObject.SetActive(false);

                        foreach (var icon in eventControl.orderIcons)
                        {
                            icon.gameObject.SetActive(false);
                        }

                        eventControl.EnsureComponent<ReorderWindowStorage>().UpdateUI(eventControl);

                        return true;
                    });

                cursor
                    .Emit(OpCodes.Brtrue, label)
                    .Emit(OpCodes.Ldarg_0);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStloc(4))
                    .EmitDelegate((int index) => index % RDEditorConstants.WindowCount);
            }
        }
    }
}
