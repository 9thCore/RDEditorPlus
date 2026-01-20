namespace RDEditorPlus.Patch.SubRows
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.SubRowsEnabled;
    }
}
