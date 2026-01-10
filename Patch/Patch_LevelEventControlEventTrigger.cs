using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RDEditorPlus.Patch
{
    [HarmonyPatch]
    internal class Patch_LevelEventControlEventTrigger
    {
        [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.OnDrag))]
        private static class OnDrag
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                const int flagLocalIndex = 13;
                const int compilerGeneratedSubclassIndex = 15;
                const int levelEventIndex = 16;
                const int rowIndex = 18;
                const int oldLevelEventTargetIndex = 25;

                MethodInfo fixFlag = typeof(OnDrag).GetMethod(nameof(FixFlag), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo fixTarget = typeof(OnDrag).GetMethod(nameof(FixTarget), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo fixY = typeof(OnDrag).GetMethod(nameof(FixY), BindingFlags.NonPublic | BindingFlags.Static);

                MethodInfo updateUI = typeof(LevelEventControl_Base).GetMethod(nameof(LevelEventControl_Base.UpdateUI));

                // Matching locals sucks
                CodeMatch matchFlag = new(
                    code => code.opcode == OpCodes.Ldloc_S
                    && code.operand is LocalBuilder { LocalIndex: flagLocalIndex });

                CodeMatch matchOldLevelEventTarget = new(
                    code => code.opcode == OpCodes.Ldloc_S
                    && code.operand is LocalBuilder { LocalIndex: oldLevelEventTargetIndex });

                CodeMatch matchLevelEvent = new(
                    code => code.opcode == OpCodes.Ldloc_S
                    && code.operand is LocalBuilder { LocalIndex: levelEventIndex });

                CodeMatch computerGeneratedSubclass = new(
                    code => code.opcode == OpCodes.Ldloc_S
                    && code.operand is LocalBuilder { LocalIndex: compilerGeneratedSubclassIndex });

                CodeMatcher matcher = new CodeMatcher(instructions)
                    .MatchForward(true, matchFlag, new CodeMatch(OpCodes.Brfalse))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, rowIndex))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixFlag))

                    .MatchForward(false, matchOldLevelEventTarget, matchLevelEvent);

                // hell
                List<Label> labels = matcher.Labels;

                matcher
                    .Insert(new CodeInstruction(OpCodes.Ldloc_S, levelEventIndex))
                    .AddLabels(labels)
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, rowIndex))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixTarget))
                    .SetInstruction(new CodeInstruction(matcher.Opcode, matcher.Operand))

                    .MatchForward(false, new CodeMatch(OpCodes.Callvirt, updateUI))
                //    .MatchBack(false, computerGeneratedSubclass)
                //    .Advance(1);

                //object eventControlField = matcher.Operand;

                //matcher
                //    .Advance(-1)
                //    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, compilerGeneratedSubclassIndex))
                //    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, eventControlField))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixY));

                return matcher.InstructionEnumeration();
            }

            private static bool FixFlag(bool flag, int row)
            {
                return SubRowStorage.Holder.GetCanDragFlagOverride(flag, row);
            }

            private static void FixTarget(LevelEvent_Base levelEvent, int row)
            {
                if (levelEvent.IsPreCreationEvent()
                    || !PluginConfig.SpriteSubRowsEnabled
                    || !SubRowStorage.Holder.TryFindSpriteForRow(row, out string id, out int _, out int subRow))
                {
                    return;
                }

                levelEvent.target = id;
                SubRowStorage.EventInfo info = SubRowStorage.Holder.GetOrCreateEventData(levelEvent);
                info.subRow = subRow;
            }

            private static LevelEventControl_Base FixY(LevelEventControl_Base control)
            {
                LevelEvent_Base levelEvent = control.levelEvent;

                if (SubRowStorage.BlacklistedFromSubRowSystem(levelEvent))
                {
                    return control;
                }

                if (levelEvent.IsPreCreationEvent())
                {
                    SubRowStorage.Holder.FixPreCreationEventData(control);
                    return control;
                }

                if (PluginConfig.RoomSubRowsEnabled && levelEvent.IsRoomEvent())
                {
                    if (!SubRowStorage.Holder.TryFindRoomForRow(levelEvent.y, out int room, out int subRow))
                    {
                        return control;
                    }

                    levelEvent.y = room;
                    SubRowStorage.EventInfo info = SubRowStorage.Holder.GetOrCreateEventData(levelEvent);
                    info.subRow = subRow;
                }

                return control;
            }
        }
    }
}
