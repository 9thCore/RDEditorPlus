namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents.Partition
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.OptimisationsEnabled
            && PluginConfig.OptimisationsTimelinePartitionsEnabled
            && PluginConfig.OptimisationsTimelineLevel.HasFlag(PluginConfig.TimelineOptimisations.ChangeParents);
    }
}
