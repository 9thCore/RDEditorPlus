namespace RDEditorPlus.Patch.SubRows.Window
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.SubRowsEnabled && PluginConfig.WindowSubRowsEnabled;
    }
}
