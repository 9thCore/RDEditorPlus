namespace RDEditorPlus.Patch.NodeEditor
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.LevelMergeEnabled;
    }
}
