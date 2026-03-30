namespace RDEditorPlus.Patch.Optimisation.OptTimeline.RecullOnlyIfRequired
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.OptimisationsEnabled
            && PluginConfig.OptimisationsTimelineLevel.HasFlag(PluginConfig.TimelineOptimisations.RecullOnlyIfRequired);
    }
}
