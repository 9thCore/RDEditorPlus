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
                        InspectorPanel panel = __instance.inspectorPanel;
                        foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                        {
                            if (!eventControl.levelEvent.isBaseEvent)
                            {
                                panel.Save(eventControl.levelEvent, isNew: true);
                                eventControl.UpdateUI();
                            }
                        }

                        PropertyStorage.Instance.UnmarkAll();
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
    }
}
