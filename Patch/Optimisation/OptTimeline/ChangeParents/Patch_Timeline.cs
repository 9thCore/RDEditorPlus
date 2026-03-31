using HarmonyLib;
using RDEditorPlus.Functionality.Optimisation;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents
{
    internal static class Patch_Timeline
    {
        [HarmonyPatch(typeof(Timeline), nameof(Timeline.Awake))]
        private static class Awake
        {
            private static void Postfix(Timeline __instance)
            {
                TimelineOptimisations.Instance.FetchData(__instance);
            }
        }
    }
}
