namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.OptimisationsEnabled
            && PluginConfig.OptimisationsTimelineLevel.HasFlag(PluginConfig.TimelineOptimisations.ChangeParents);
    }
}
