using HarmonyLib;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.RecullOnlyIfRequired
{
    internal static class Patch_Timeline
    {
        [HarmonyPatch(typeof(Timeline), nameof(Timeline.Awake))]
        private static class Awake
        {
            private static void Postfix()
            {
                lastPosition = new Vector2(float.NaN, float.NaN);
            }
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.CullMaskedObjects))]
        private static class CullMaskedObjects
        {
            private static bool Prefix(Timeline __instance)
            {
                if (__instance.shouldUpdateUI || lastPosition != CurrentPosition)
                {
                    lastPosition = CurrentPosition;
                    return true;
                }

                return false;
            }
        }

        private static Vector2 lastPosition;
        private static Vector2 CurrentPosition => scnEditor.instance.timeline.scrollviewContent.anchoredPosition;
    }
}
