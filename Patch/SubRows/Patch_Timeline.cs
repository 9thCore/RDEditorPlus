using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_Timeline
    {
        [HarmonyPatch(typeof(Timeline), nameof(Timeline.UpdateUIInternalCo))]
        private static class UpdateUIInternalCo
        {
            // This is how coroutines are correctly patched, apparently
            private static IEnumerator Postfix(IEnumerator __result)
            {
                while (__result.MoveNext())
                {
                    GeneralManager.Instance.UpdateTabPanelOnly();
                    yield return __result.Current;
                }
            }

            [HarmonyPatch(MethodType.Enumerator)]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                FieldInfo field = typeof(Timeline).GetField(nameof(Timeline.scrollViewVertContent));
                MethodInfo method = typeof(UpdateUIInternalCo).GetMethod(nameof(CorrectMaxUsedY), BindingFlags.NonPublic | BindingFlags.Static);

                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(code => code.LoadsField(field)))
                    .ThrowIfInvalid($"Could not find where {nameof(Timeline.scrollViewVertContent)} is read, can't apply {nameof(CorrectMaxUsedY)} patch.")
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, method))

                    .InstructionEnumeration();
            }

            private static void CorrectMaxUsedY()
            {
                scnEditor.instance.timeline.maxUsedY = GeneralManager.Instance.GetCurrentTabMaxUsedY();
            }
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.UpdateTimelineHeight))]
        private static class UpdateTimelineHeight
        {
            private static void Postfix()
            {
                GeneralManager.Instance.UpdateTabPanelOnly();
            }
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.LateUpdate))]
        private static class LateUpdate
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo method = typeof(LateUpdate).GetMethod(nameof(GetActualValue), BindingFlags.NonPublic | BindingFlags.Static);

                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Stloc_2))
                    .ThrowIfInvalid($"Could not find where the local variable is set, can't apply {nameof(GetActualValue)} patch.")
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, method))

                    .InstructionEnumeration();
            }

            private static int GetActualValue(int currentValue)
            {
                switch (scnEditor.instance.currentTab)
                {
                    case Tab.Sprites:
                        if (!PluginConfig.SpriteSubRowsEnabled)
                        {
                            return currentValue;
                        }

                        return Mathf.Min(scnEditor.instance.timeline.maxUsedY, scnEditor.instance.timeline.scaledRowCellCount - 2);
                    case Tab.Rooms:
                        if (!PluginConfig.RoomSubRowsEnabled)
                        {
                            return currentValue;
                        }

                        return scnEditor.instance.timeline.maxUsedY;
                    default:
                        return currentValue;
                }
            }
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.ApplyNewMaxUsedY))]
        private static class ApplyNewMaxUsedY
        {
            private static void Prefix()
            {
                scnEditor.instance.timeline.maxUsedY = GeneralManager.Instance.GetCurrentTabMaxUsedY();
            }
        }

        [HarmonyPatch(typeof(Timeline), $"get_{nameof(Timeline.usedRowCount)}")]
        private static class get_usedRowCount
        {
            private static void Postfix(ref int __result)
            {
                GeneralManager.Instance.OverrideUsedRowCount(ref __result);
            }
        }
    }
}
