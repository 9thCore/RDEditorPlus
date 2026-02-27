namespace RDEditorPlus.Patch.Windows.MoreWindows.SubRowDisabled
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.WindowsEnabled && PluginConfig.WindowsMoreEnabled
            && !(PluginConfig.SubRowsEnabled && PluginConfig.WindowSubRowsEnabled);
    }
}
