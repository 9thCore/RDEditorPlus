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
                MethodInfo rowEventSpecificFix = typeof(OnPointerClick).GetMethod(nameof(RowEventSpecificFuckassFix), BindingFlags.NonPublic | BindingFlags.Static);
                FieldInfo levelEventY = typeof(LevelEvent_Base).GetField(nameof(LevelEvent_Base.y));

                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, modifyY))

                    .MatchForward(false, new CodeMatch(OpCodes.Stfld, levelEventY))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 12))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, rowEventSpecificFix))

                    .InstructionEnumeration();
            }

            private static int ModifyY(int y)
            {
                return GeneralManager.Instance.ModifyPointerClickYPosition(y);
            }

            private static int RowEventSpecificFuckassFix(int _, LevelEventControl_Base eventControl)
            {
                return GeneralManager.Instance.RowEventYFix(scnEditor.instance.timeline.cellPointedByMouse.y, eventControl);
            }
        }
    }
}
