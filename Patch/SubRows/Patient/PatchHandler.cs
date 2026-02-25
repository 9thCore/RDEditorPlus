namespace RDEditorPlus.Patch.SubRows.Patient
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.SubRowsEnabled && PluginConfig.PatientSubRowsEnabled;
    }
}
