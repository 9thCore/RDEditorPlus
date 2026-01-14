using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_TimelineEventTrigger
    {
        [HarmonyPatch(typeof(TimelineEventTrigger), nameof(TimelineEventTrigger.OnPointerClick))]
        private static class OnPointerClick
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo modifyY = typeof(OnPointerClick).GetMethod(nameof(ModifyY), BindingFlags.NonPublic | BindingFlags.Static);

                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                    .ThrowIfInvalid(", h")
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, modifyY))

                    .InstructionEnumeration();
            }

            private static int ModifyY(int y)
            {
                return GeneralManager.Instance.ModifyPointerClickYPosition(y);
            }
        }
    }
}
