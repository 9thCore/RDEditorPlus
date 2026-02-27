using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDLevelEditor;
using System.Reflection;

namespace RDEditorPlus.Patch.Windows.MoreWindows.SubRowDisabled
{
    internal static class Patch_LevelEventControlEventTrigger
    {
        [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.OnDrag))]
        private static class OnDrag
        {
            private static void ILManipulator(ILContext il, MethodBase original, ILLabel retLabel)
            {
                const byte offsetIndex = 12;
                const byte flagIndex = 13;

                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchLdloc(flagIndex))
                    .Emit(OpCodes.Ldloc_S, offsetIndex)
                    .EmitDelegate((bool originalFlag, int offset) =>
                    {
                        int change = offset / scnEditor.instance.cellHeight;

                        foreach (LevelEventControl_Base control in scnEditor.instance.selectedControls)
                        {
                            int newRow = control.levelEvent.y + change;

                            if (newRow < 0 || newRow >= MoreWindowManager.Instance.WindowCount)
                            {
                                return false;
                            }
                        }

                        return true;
                    });
            }
        }
    }
}
