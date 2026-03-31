using HarmonyLib;
using RDEditorPlus.Functionality.Optimisation;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents
{
    internal static class Patch_TabSection
    {
        [HarmonyPatch(typeof(TabSection), nameof(TabSection.ChangePage))]
        private static class ChangePage
        {
            private static void Prefix(TabSection __instance) => TimelineOptimisations.Instance.PreChangePage(__instance);
            private static void Postfix(TabSection __instance) => TimelineOptimisations.Instance.PostChangePage(__instance);
        }
    }
}
