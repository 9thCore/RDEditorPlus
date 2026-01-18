using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;
using System.Reflection;

namespace RDEditorPlus.Patch.SubRows
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
                const byte rowIndex = 18;

                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchLdloc(flagIndex))
                    .Emit(OpCodes.Ldloc_S, offsetIndex)
                    .EmitDelegate((bool originalFlag, int offset) =>
                    {
                        return GeneralManager.Instance.CanAllSelectedEventsBeDragged(
                            originalFlag, offset / scnEditor.instance.cellHeight);
                    });

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.OpCode == OpCodes.Stfld)
                    .EmitDelegate((LevelEventControl_Base eventControl) =>
                    {
                        OnDrag.eventControl = eventControl;
                        return eventControl;
                    });

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStloc(rowIndex))
                    .EmitDelegate((int oldY) =>
                    {
                        return GeneralManager.Instance.GetDraggedEventYPosition(eventControl, oldY);
                    });
            }

            private static LevelEventControl_Base eventControl;
        }
    }
}
