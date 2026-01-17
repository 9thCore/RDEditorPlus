using HarmonyLib;
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
        [HarmonyAfter(Plugin.RDModificationsGUID)]
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
                scnEditor.instance.timeline.maxUsedY = GeneralManager.Instance.GetCurrentTabMaxUsedY() ?? scnEditor.instance.timeline.maxUsedY;
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
        [HarmonyAfter(Plugin.RDModificationsGUID)]
        private static class LateUpdate
        {
            private static void Postfix(Timeline __instance)
            {
                int? enabledRows = GeneralManager.Instance.GetTimelineDisabledRowsValueThing();
                if (enabledRows == null)
                {
                    return;
                }

                float height = (__instance.scaledRowCellCount - enabledRows.Value) * __instance.cellHeight;
                __instance.disabledRowsQuad.GetComponent<RectTransform>().SizeDeltaY(height);
            }
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.ApplyNewMaxUsedY))]
        [HarmonyAfter(Plugin.RDModificationsGUID)]
        private static class ApplyNewMaxUsedY
        {
            private static void Prefix()
            {
                scnEditor.instance.timeline.maxUsedY = GeneralManager.Instance.GetCurrentTabMaxUsedY() ?? scnEditor.instance.timeline.maxUsedY;
            }
        }

        [HarmonyPatch(typeof(Timeline), $"get_{nameof(Timeline.usedRowCount)}")]
        [HarmonyAfter(Plugin.RDModificationsGUID)]
        private static class get_usedRowCount
        {
            private static void Postfix(ref int __result)
            {
                GeneralManager.Instance.OverrideUsedRowCount(ref __result);
            }
        }
    }
}
