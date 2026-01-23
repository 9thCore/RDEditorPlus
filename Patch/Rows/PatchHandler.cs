namespace RDEditorPlus.Patch.Rows
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.RowsEnabled;
    }
}
