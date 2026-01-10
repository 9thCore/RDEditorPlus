using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RDEditorPlus.Patch.SubRows
{
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

                MethodInfo fixEvent = typeof(SetLevelEventControlType)
                    .GetMethod(nameof(FixEvent), BindingFlags.NonPublic | BindingFlags.Static);

                CodeMatch matchSpriteDataIndex = new(
                    code => code.opcode == OpCodes.Stloc_S
                    && code.operand is LocalBuilder { LocalIndex: makeSpriteIndex });

                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, fixEvent))

                    .InstructionEnumeration();
            }

            private static void FixEvent(LevelEvent_Base levelEvent)
            {
                SubRowStorage.Holder.SetupEvent(levelEvent);
            }

            private static void FixSprite(LevelEvent_Base levelEvent)
            {
                if (!PluginConfig.SpriteSubRowsEnabled
                    || !SubRowStorage.Holder.TryFindSpriteForRow(levelEvent.y, out _, out int roomPosition, out int subRow))
                {
                    return;
                }

                // The vanilla code sets the target anyway, no need to bother
                // levelEvent.target = id;
                SubRowStorage.EventInfo info = SubRowStorage.Holder.GetOrCreateEventData(levelEvent);
                info.subRow = subRow;
                levelEvent.y = roomPosition;
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.ShowTabSection))]
        private static class ShowTabSection
        {
            private static void Postfix()
            {
                SubRowStorage.Holder.UpdateCurrentTabSubRowUI(force: true);
            }
        }
    }
}
