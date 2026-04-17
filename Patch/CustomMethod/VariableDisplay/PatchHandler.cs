namespace RDEditorPlus.Patch.CustomMethod.VariableDisplay
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsVariableDisplayEnabled;
    }
}
