namespace RDEditorPlus.Patch.LevelMerger
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.LevelMergeEnabled;
    }
}
