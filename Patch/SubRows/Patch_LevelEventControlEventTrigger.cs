using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_LevelEventControlEventTrigger
    {
        [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.OnDrag))]
        private static class OnDrag
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                const int offsetIndex = 12;
                const int flagIndex = 13;
                const int rowIndex = 18;

                MethodInfo fixFlag = typeof(OnDrag).GetMethod(nameof(FixFlag), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo fixY = typeof(OnDrag).GetMethod(nameof(FixY), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo fetchEventControl = typeof(OnDrag).GetMethod(nameof(FetchEventControl), BindingFlags.NonPublic | BindingFlags.Static);

                CodeMatch matchRow = new(
                    code => code.opcode == OpCodes.Stloc_S
                    && code.operand is LocalBuilder { LocalIndex: rowIndex });

                CodeMatch matchFlag = new(
                    code => code.opcode == OpCodes.Ldloc_S
                    && code.operand is LocalBuilder { LocalIndex: flagIndex });

                return new CodeMatcher(instructions)
                    .MatchForward(true, matchFlag, new CodeMatch(OpCodes.Brfalse))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, offsetIndex))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixFlag))

                    .MatchForward(false, new CodeMatch(OpCodes.Stfld))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fetchEventControl))

                    .MatchForward(false, matchRow)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixY))

                    .InstructionEnumeration();
            }

            // I don't really like this, but it's better than trying to fetch it from
            // the compiler generated subclass somehow-or-other
            private static LevelEventControl_Base FetchEventControl(LevelEventControl_Base eventControl)
            {
                OnDrag.eventControl = eventControl;
                return eventControl;
            }

            private static bool FixFlag(bool originalFlag, int offset)
            {
                return GeneralManager.Instance.CanAllSelectedEventsBeDragged(originalFlag, offset);
            }

            private static int FixY(int oldY)
            {
                return GeneralManager.Instance.GetDraggedEventYPosition(eventControl, oldY);
            }

            private static LevelEventControl_Base eventControl;
        }
    }
}
