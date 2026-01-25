using HarmonyLib;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_LevelEventControlEventTrigger
    {
        [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.UpdateInspectorPanelIfNecessary))]
        private static class UpdateInspectorPanelIfNecessary
        {
            private static void Postfix()
            {
                if (scnEditor.instance.selectedControls.Count > 1)
                {
                    LevelEvent_Base levelEvent = scnEditor.instance.selectedControls[0].levelEvent;

                    if (!levelEvent.isBaseEvent)
                    {
                        levelEvent.inspectorPanel.UpdateUI(levelEvent);
                    }
                }
            }
        }
    }
}
