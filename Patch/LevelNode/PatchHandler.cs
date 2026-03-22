namespace RDEditorPlus.Patch.LevelNode
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.NodeEditorEnabled && PluginConfig.LevelNodeEnabled;
    }
}
