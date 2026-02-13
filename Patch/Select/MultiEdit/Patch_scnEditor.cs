using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Linq;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Start))]
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DecodeData))]
        private static class _Start_DecodeData_
        {
            private static void Postfix()
            {
                EventUtil.UpdateVFXPresetDropdown();
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
    }
}
