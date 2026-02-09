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

                scnEditor.instance.selectedControls[0].ShowDataOnInspector();

                LevelEventControl_Base eventControl = scnEditor.instance.selectedControls[0];

                int bar = eventControl.bar;
                string beat = eventControl.beat.ToString();

                InspectorPanel panel = eventControl.levelEvent.inspectorPanel;

                if (scnEditor.instance.selectedControls.Any(eventControl => eventControl.bar != bar))
                {
                    ((Text)panel.position.bar.placeholder).text = InspectorUtil.MixedTextBar;
                    panel.position.bar.text = string.Empty;
                }

                if (scnEditor.instance.selectedControls.Any(eventControl => eventControl.beat.ToString() != beat))
                {
                    ((Text)panel.position.beat.placeholder).text = InspectorUtil.MixedTextBeat;
                    panel.position.beat.text = string.Empty;
                }

                return false;
            }
        }
    }
}
