namespace RDEditorPlus.Patch.CustomMethod.VariableAlias
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsVariableAliasEnabled;
    }
}
