using HarmonyLib;
using RDEditorPlus.ExtraData;
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
                const int levelEventControlIndex = 14;

                MethodInfo modifyY = typeof(OnPointerClick).GetMethod(nameof(ModifyY), BindingFlags.NonPublic | BindingFlags.Static);

                CodeMatch matchLevelEventControl = new(
                    code => code.opcode == OpCodes.Stloc_S
                    && code.operand is LocalBuilder { LocalIndex: levelEventControlIndex });

                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                    .ThrowIfInvalid(", h")
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, modifyY))

                    .InstructionEnumeration();
            }

            private static int ModifyY(int y)
            {
                return SubRowStorage.Holder.ModifyPlacementY(y);
            }
        }
    }
}
