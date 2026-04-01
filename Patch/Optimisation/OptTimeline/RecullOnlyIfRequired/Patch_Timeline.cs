using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.RecullOnlyIfRequired
{
    internal static class Patch_Timeline
    {
        [HarmonyPatch(typeof(Timeline), nameof(Timeline.Awake))]
        private static class Awake
        {
            private static void Postfix() => storage = new();
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.CullMaskedObjects))]
        private static class CullMaskedObjects
        {
            private static bool Prefix(Timeline __instance) => storage.ShouldUpdate(__instance);
        }

        private static TimelineLazyUpdateStorage storage;
    }
}
