namespace RDEditorPlus.Patch.CustomMethod
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsAutocomplete != PluginConfig.CustomMethodAutocompleteBehaviour.Disabled;
    }
}
