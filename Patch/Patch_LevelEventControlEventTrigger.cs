using HarmonyLib;
using RDEditorPlus.ExtraData;
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
                const int levelEventIndex = 16;
                const int rowIndex = 18;
                const int oldLevelEventTargetIndex = 25;

                MethodInfo fixFlag = typeof(OnDrag).GetMethod(nameof(FixFlag), BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo fixTarget = typeof(OnDrag).GetMethod(nameof(FixTarget), BindingFlags.NonPublic | BindingFlags.Static);

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

                CodeMatcher matcher = new CodeMatcher(instructions)
                    .MatchForward(true, matchFlag, new CodeMatch(OpCodes.Brfalse))
                    .ThrowIfInvalid($"???")
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, rowIndex))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixFlag))

                    .MatchForward(false, matchOldLevelEventTarget, matchLevelEvent)
                    .ThrowIfInvalid($"??? (2)");

                // hell
                List<Label> labels = matcher.Labels;

                matcher
                    .Insert(new CodeInstruction(OpCodes.Ldloc_S, levelEventIndex))
                    .AddLabels(labels)
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, rowIndex))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixTarget))
                    .SetInstruction(new CodeInstruction(matcher.Opcode, matcher.Operand));

                return matcher.InstructionEnumeration();
            }

            private static bool FixFlag(bool flag, int row)
            {
                switch (scnEditor.instance.currentTab)
                {
                    case Tab.Sprites:
                        return PluginConfig.SpriteSubRowsEnabled ? SubRowStorage.Holder.TryFindSpriteForRow(row, out _, out _) : flag;
                    default:
                        return flag;
                }
            }

            private static void FixTarget(LevelEvent_Base levelEvent, int row)
            {
                if (!PluginConfig.SpriteSubRowsEnabled
                    || !SubRowStorage.Holder.TryFindSpriteForRow(row, out string id, out int subRow))
                {
                    return;
                }

                levelEvent.target = id;
                SubRowStorage.EventInfo info = SubRowStorage.Holder.GetOrCreateEventData(levelEvent);
                info.subRow = subRow;
            }
        }
    }
}
