using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Awake))]
        private static class Awake
        {
            private static void Prefix()
            {
                GeneralManager.RemoveInstance();
                RoomManager.RemoveInstance();
                RowManager.RemoveInstance();
                SpriteManager.RemoveInstance();
                WindowManager.RemoveInstance();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DeleteAllData))]
        private static class DeleteAllData
        {
            private static void Prefix()
            {
                SubRowStorage.Instance.Clear();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.UndoOrRedo))]
        private static class UndoOrRedo
        {
            private static void Postfix()
            {
                scnEditor.instance.timeline.maxUsedY = GeneralManager.Instance.GetCurrentTabMaxUsedY() ?? scnEditor.instance.timeline.maxUsedY;
                GeneralManager.Instance.UpdateTab(force: true);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.UpdateTimelineAccordingToLevelEventType))]
        private static class UpdateTimelineAccordingToLevelEventType
        {
            private static void Prefix()
            {
                GeneralManager.Instance.UpdateTab(force: false);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SetLevelEventControlType))]
        private static class SetLevelEventControlType
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo setupJustCreatedEvent = typeof(SetLevelEventControlType)
                    .GetMethod(nameof(SetupJustCreatedEvent), BindingFlags.NonPublic | BindingFlags.Static);

                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, setupJustCreatedEvent))

                    .InstructionEnumeration();
            }

            private static void SetupJustCreatedEvent(LevelEvent_Base levelEvent)
            {
                GeneralManager.Instance.SetupJustCreatedEvent(levelEvent);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.ShowTabSection))]
        private static class ShowTabSection
        {
            private static void Prefix(Tab tab)
            {
                GeneralManager.Instance.ChangeTab(tab);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SwapSpritePositions))]
        private static class SwapSpritePositions
        {
            private static void Postfix(LevelEvent_MakeSprite a, LevelEvent_MakeSprite b)
            {
                SubRowStorage.Instance.UpdateSpriteUsedSubRowCountIfRequired(a.spriteId, 0);
                SubRowStorage.Instance.UpdateSpriteUsedSubRowCountIfRequired(b.spriteId, 0);

                SpriteManager.Instance.UpdateTab(force: false);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.AddNewRow))]
        private static class AddNewRow
        {
            private static void Postfix()
            {
                if (!PluginConfig.PatientSubRowsEnabled)
                {
                    return;
                }

                RowManager.Instance.UpdateTab(force: false);
                scnEditor.instance.timeline.UpdateMaxUsedY();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.MoveRowVertically))]
        private static class MoveRowVertically
        {
            private static void Postfix()
            {
                if (!PluginConfig.PatientSubRowsEnabled)
                {
                    return;
                }

                RowManager.Instance.UpdateTab(force: false);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Paste), new System.Type[] { typeof(bool) })]
        private static class Paste
        {
            private static void Postfix()
            {
                GeneralManager.Instance.UpdateTab(force: false);
            }
        }
    }
}
