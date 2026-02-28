using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SetLevelEventControlType))]
        private static class SetLevelEventControlType
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchCallOrCallvirt<LevelEvent_Base>(nameof(LevelEvent_Base.OnCreate)))
                    .Emit(OpCodes.Ldarg_1)
                    .Emit(OpCodes.Ldloc_2)
                    .EmitDelegate((LevelEventType levelEventType, LevelEvent_Base levelEvent) =>
                    {
                        if (levelEventType != LevelEventType.ReorderWindows)
                        {
                            return;
                        }

                        int windows = MoreWindowManager.Instance.WindowCount;
                        int[] order = new int[windows];
                        for (int i = 0; i < windows; i++)
                        {
                            order[i] = i;
                        }

                        (levelEvent as LevelEvent_ReorderWindows).order = order;
                    });
            }

            //private static void Postfix(LevelEventType levelEventType)
            //{
            //    if (levelEventType == LevelEventType.ReorderWindows)
            //    {
            //        var levelEvent = (LevelEvent_ReorderWindows) scnEditor.instance.selectedControl.levelEvent;

            //        int windows = MoreWindowManager.Instance.WindowCount;
            //        int[] order = new int[windows];
            //        for (int i = 0; i < windows; i++)
            //        {
            //            order[i] = i;
            //        }

            //        levelEvent.order = order;
            //    }
            //}
        }
    }
}
