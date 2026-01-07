using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RDEditorPlus.Patch
{
    [HarmonyPatch]
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DeleteAllData))]
        private static class DeleteAllData
        {
            private static void Prefix()
            {
                SubRowStorage.Holder.Clear();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.UndoOrRedo))]
        private static class UndoOrRedo
        {
            private static void Postfix()
            {
                SubRowStorage.Holder.CorrectMaxUsedY();
                SubRowStorage.Holder.UpdateCurrentTabSubRowUI(true);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.AddNewEventControl))]
        private static class AddNewEventControl
        {
            private static void Postfix(LevelEventControl_Base eventControl)
            {
                SubRowStorage.Holder.HandleNewLevelEventControl(eventControl);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.UpdateTimelineAccordingToLevelEventType))]
        private static class UpdateTimelineAccordingToLevelEventType
        {
            private static void Prefix()
            {
                SubRowStorage.Holder.UpdateCurrentTabSubRowUI();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SetLevelEventControlType))]
        private static class SetLevelEventControlType
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                const int makeSpriteIndex = 5;

                MethodInfo fixSprite = typeof(SetLevelEventControlType)
                    .GetMethod(nameof(FixSprite), BindingFlags.NonPublic | BindingFlags.Static);

                CodeMatch matchSpriteDataIndex = new(
                    code => code.opcode == OpCodes.Stloc_S
                    && code.operand is LocalBuilder { LocalIndex: makeSpriteIndex });

                return new CodeMatcher(instructions)
                    .MatchForward(false, matchSpriteDataIndex)
                    .ThrowIfInvalid("og")
                    .MatchBack(false, new CodeMatch(OpCodes.Ldarg_0))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixSprite))

                    .InstructionEnumeration();
            }

            private static void FixSprite(LevelEvent_Base levelEvent)
            {
                if (!PluginConfig.SpriteSubRowsEnabled
                    || !SubRowStorage.Holder.TryFindSpriteForRow(levelEvent.y, out string id, out int subRow))
                {
                    return;
                }

                // The vanilla code sets the target anyway, no need to bother
                // levelEvent.target = id;
                SubRowStorage.EventInfo info = SubRowStorage.Holder.GetOrCreateEventData(levelEvent);
                info.subRow = subRow;

                int idx = 0;
                foreach (LevelEvent_MakeSprite sprite in scnEditor.instance.currentPageSpritesData)
                {
                    if (sprite.spriteId == id)
                    {
                        levelEvent.y = idx;
                        return;
                    }

                    idx++;
                }
            }
        }
    }
}
