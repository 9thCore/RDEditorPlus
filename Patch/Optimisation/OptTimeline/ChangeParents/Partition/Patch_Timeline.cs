using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.Optimisation;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents.Partition
{
    internal static class Patch_Timeline
    {
        [HarmonyPatch(typeof(Timeline), nameof(Timeline.Awake))]
        private static class Awake
        {
            private static void Postfix() => storage = new();
        }

        [HarmonyPatch(typeof(Timeline), nameof(Timeline.LateUpdate))]
        private static class LateUpdate
        {
            private static void Prefix(Timeline __instance)
            {
                if (storage.ShouldUpdate(__instance))
                {
                    var anchoredPosition = __instance.scrollviewContent.anchoredPosition;
                    var leftEdge = __instance.GetBarAndBeatWithPosX(-anchoredPosition.x);
                    var rightEdge = __instance.GetBarAndBeatWithPosX(-anchoredPosition.x + __instance.scrollview.rect.width);

                    TimelineOptimisations.Instance.HandlePartitionParents(
                        __instance.editor.currentTabSection,
                        leftEdge.bar - PluginConfig.OptimisationsTimelinePartitionsBuffer,
                        rightEdge.bar + PluginConfig.OptimisationsTimelinePartitionsBuffer);
                }
            }
        }

        private static TimelineLazyUpdateStorage storage;
    }
}
