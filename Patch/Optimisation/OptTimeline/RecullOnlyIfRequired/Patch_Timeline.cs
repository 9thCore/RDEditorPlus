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
                lastTab = Tab.None;
            }
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.CullMaskedObjects))]
        private static class CullMaskedObjects
        {
            private static bool Prefix(Timeline __instance)
            {
                if (__instance.shouldUpdateUI || lastTab != CurrentTab 
                    || lastPosition != CurrentPosition || lastPage != CurrentPage)
                {
                    lastPosition = CurrentPosition;
                    lastTab = CurrentTab;
                    lastPage = CurrentPage;
                    return true;
                }

                return false;
            }
        }

        private static Vector2 lastPosition;
        private static Tab lastTab;
        private static int lastPage;
        private static Vector2 CurrentPosition => scnEditor.instance.timeline.scrollviewContent.anchoredPosition;
        private static Tab CurrentTab => scnEditor.instance.currentTab;
        private static int CurrentPage => scnEditor.instance.currentTabSection.pageIndex;
    }
}
