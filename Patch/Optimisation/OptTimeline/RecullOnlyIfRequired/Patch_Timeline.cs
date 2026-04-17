using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
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
            private static bool Prefix(Timeline __instance)
            {
                if (LevelUtil.ForceEventRecull)
                {
                    LevelUtil.ForceEventRecull = false;
                    storage.ShouldUpdate(__instance);
                    return true;
                }

                return storage.ShouldUpdate(__instance);
            }
        }

        private static TimelineLazyUpdateStorage storage;
    }
}
