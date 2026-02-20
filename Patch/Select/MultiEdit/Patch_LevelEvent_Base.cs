using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_LevelEvent_Base
    {
        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.SaveData))]
        private static class SaveData
        {
            private static bool Prefix(LevelEvent_Base __instance, ref bool __state)
            {
                if (!InspectorUtil.CanMultiEdit())
                {
                    __state = true;
                    return true;
                }

                __state = false;
                if (__instance.inspectorPanel != null)
                {
                    using (new SaveStateScope(clearRedo: true, skipSaving: false, skipTimelinePos: false))
                    {
                        PropertyStorage.Instance.skipUpdatingPropertyUI = true;

                        InspectorPanel panel = __instance.inspectorPanel;
                        bool setRow = panel.levelEventInfo.showsRowControl && PropertyStorage.Instance.rowChanged;
                        int row = setRow ? panel.row.dropdown.value + panel.levelEventInfo.attribute.defaultRow : 0;

                        foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                        {
                            if (!eventControl.levelEvent.isBaseEvent)
                            {
                                if (setRow)
                                {
                                    eventControl.levelEvent.row = row;
                                }

                                panel.SaveProperties(eventControl.levelEvent);
                                eventControl.UpdateUI();
                            }
                        }

                        PropertyStorage.Instance.UnmarkAll();
                        PropertyStorage.Instance.skipUpdatingPropertyUI = false;

                        PropertyStorage.Instance.scrollToTopOnUpdate = false;
                        panel.UpdateUI(scnEditor.instance.selectedControls[0].levelEvent);
                        PropertyStorage.Instance.scrollToTopOnUpdate = true;
                    }
                }

                return false;
            }

            private static void Postfix(bool __state)
            {
                if (__state)
                {
                    PropertyStorage.Instance.UnmarkAll();
                }
            }
        }

        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.SetConditional))]
        private static class SetConditional
        {
            private static void Postfix(LevelEvent_Base __instance, int conditionalId, string gid, bool? state)
            {
                if (calling
                    || !InspectorUtil.CanMultiEdit())
                {
                    return;
                }

                calling = true;

                foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                {
                    if (eventControl.levelEvent != __instance)
                    {
                        eventControl.levelEvent.SetConditional(conditionalId, gid, state);
                        eventControl.UpdateUIInternal();
                    }
                }

                calling = false;
            }

            private static bool calling = false;
        }
    }
}
