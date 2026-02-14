using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Start))]
        private static class Start
        {
            private static void Postfix()
            {
                EventUtil.UpdateVFXPresetDropdown();
                PropertyStorage.Instance.UpdateRowPropertyControls();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DecodeData))]
        private static class DecodeData
        {
            private static void Postfix()
            {
                EventUtil.UpdateVFXPresetDropdown();
                PropertyStorage.Instance.UpdateRowPropertyControls();
            }
        }

        [HarmonyPatch(typeof(scnEditor), $"get_{nameof(scnEditor.selectedControl)}")]
        private static class getSelectedControl
        {
            private static bool Prefix(scnEditor __instance, ref LevelEventControl_Base __result)
            {
                if (__instance.selectedControls.Count == 0)
                {
                    __result = null;
                    return false;
                }

                __result = __instance.selectedControls[0];
                return false;
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.InspectorPanel_UpdateUI))]
        private static class InspectorPanel_UpdateUI
        {
            private static bool Prefix()
            {
                if (!InspectorUtil.CanMultiEdit())
                {
                    return true;
                }

                var control = scnEditor.instance.selectedControls[0];
                var panel = control.levelEvent.inspectorPanel;

                panel.UpdateUIProperties(control.levelEvent);
                panel.position.MultiEditUpdateUI();

                return false;
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.AddNewRow))]
        private static class AddNewRow
        {
            private static void Postfix()
            {
                PropertyStorage.Instance.ScheduleRowPropertyControlsUpdate();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DeleteRowClick))]
        private static class DeleteRowClick
        {
            private static void Postfix()
            {
                PropertyStorage.Instance.UpdateRowPropertyControls();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SwapRowPositions))]
        private static class SwapRowPositions
        {
            private static void Postfix()
            {
                PropertyStorage.Instance.ScheduleRowPropertyControlsUpdate();
            }
        }
    }
}
