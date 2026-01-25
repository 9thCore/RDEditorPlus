namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.SelectionEnabled && PluginConfig.SelectionMultiEditEnabled;
    }
}
