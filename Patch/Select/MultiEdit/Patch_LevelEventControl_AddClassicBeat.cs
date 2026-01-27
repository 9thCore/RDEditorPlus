using HarmonyLib;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_LevelEventControl_AddClassicBeat
    {
        [HarmonyPatch(typeof(LevelEventControl_AddClassicBeat), nameof(LevelEventControl_AddClassicBeat.ShowBorders))]
        private static class ShowBorders
        {
            private static void Postfix(LevelEventControl_AddClassicBeat __instance, bool show)
            {
                if (callingMethodForEveryoneElse)
                {
                    return;
                }

                if (!scnEditor.instance.selectedControls.Contains(__instance))
                {
                    return;
                }

                callingMethodForEveryoneElse = true;

                foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                {
                    if (__instance != eventControl
                        && eventControl is LevelEventControl_AddClassicBeat addClassicEventControl)
                    {
                        addClassicEventControl.ShowBorders(show);
                    }
                }

                callingMethodForEveryoneElse = false;
            }

            private static bool callingMethodForEveryoneElse = false;
        }
    }
}
