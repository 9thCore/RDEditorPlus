namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.WindowsEnabled && PluginConfig.WindowsMoreEnabled;
    }
}
