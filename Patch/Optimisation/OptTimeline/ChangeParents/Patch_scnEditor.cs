using HarmonyLib;
using RDEditorPlus.Functionality.Optimisation;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Start))]
        private static class Start
        {
            private static void Prefix(scnEditor __instance)
            {
                foreach (var section in __instance.tabSections)
                {
                    TimelineOptimisations.Instance.Unparent(section, entireTab: true);
                }
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.ShowTabSection))]
        private static class ShowTabSection
        {
            private static void Postfix(scnEditor __instance, Tab tab)
            {
                TimelineOptimisations.Instance.Parent(__instance.currentTabSection, entireTab: false);
                foreach (var section in __instance.tabSections)
                {
                    if (section.tab != tab)
                    {
                        TimelineOptimisations.Instance.Unparent(section, entireTab: true);
                    }
                }
            }
        }
    }
}
