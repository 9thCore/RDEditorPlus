using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RDEditorPlus.Patch
{
    [HarmonyPatch]
    internal class Patch_TimelineEventTrigger
    {
        [HarmonyPatch(typeof(TimelineEventTrigger), nameof(TimelineEventTrigger.OnPointerClick))]
        private static class OnPointerClick
        {
            //private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            //{
            //    const int spriteDataIndexIndex = 16;
            //    const int spriteDataIndex2Index = 17;

            //    MethodInfo fixSpriteIndex = typeof(OnPointerClick).GetMethod(nameof(FixSpriteIndex), BindingFlags.NonPublic | BindingFlags.Static);

            //    CodeMatch matchSpriteDataIndex = new(
            //        code => code.opcode == OpCodes.Stloc_S
            //        && code.operand is LocalBuilder { LocalIndex: spriteDataIndexIndex });

            //    CodeMatch matchSpriteDataIndex2 = new(
            //        code => code.opcode == OpCodes.Stloc_S
            //        && code.operand is LocalBuilder { LocalIndex: spriteDataIndex2Index });

            //    return new CodeMatcher(instructions)
            //        .MatchForward(false, matchSpriteDataIndex)
            //        .Advance(1)
            //        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, spriteDataIndexIndex))
            //        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
            //        .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixSpriteIndex))
            //        .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, spriteDataIndexIndex))

            //        .MatchForward(false, matchSpriteDataIndex2)
            //        .Advance(1)
            //        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, spriteDataIndex2Index))
            //        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
            //        .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixSpriteIndex))
            //        .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, spriteDataIndex2Index))

            //        .InstructionEnumeration();
            //}

            //private static int FixSpriteIndex(int originalIndex, int y)
            //{
            //    if (!PluginConfig.SpriteSubRowsEnabled
            //        || !SubRowStorage.Holder.TryFindSpriteForRow(y, out string id, out _))
            //    {
            //        return originalIndex;
            //    }

            //    return SpriteHeader.GetSpriteDataIndex(id);
            //}

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                const int levelEventControlIndex = 14;

                MethodInfo modifyY = typeof(OnPointerClick).GetMethod(nameof(ModifyY), BindingFlags.NonPublic | BindingFlags.Static);
                //MethodInfo applySubRow = typeof(OnPointerClick)
                //    .GetMethod(nameof(ApplyStoredSubRow), BindingFlags.NonPublic | BindingFlags.Static);

                CodeMatch matchLevelEventControl = new(
                    code => code.opcode == OpCodes.Stloc_S
                    && code.operand is LocalBuilder { LocalIndex: levelEventControlIndex });

                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                    .ThrowIfInvalid(", h")
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, modifyY))

                    //.MatchForward(false, matchLevelEventControl)
                    //.ThrowIfInvalid(", h (two! tonk-tonk)")
                    //.InsertAndAdvance(new CodeInstruction(OpCodes.Call, applySubRow))

                    .InstructionEnumeration();
            }

            private static int ModifyY(int y)
            {
                return SubRowStorage.Holder.ModifyPlacementY(y);
            }

            //private static LevelEventControl_Base ApplyStoredSubRow(LevelEventControl_Base control)
            //{
            //    SubRowStorage.Holder.ApplyStoredSubRow(control);
            //    return control;
            //}
        }
    }
}
