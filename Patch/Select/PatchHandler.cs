namespace RDEditorPlus.Patch.Select
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.SelectionEnabled;
    }
}
