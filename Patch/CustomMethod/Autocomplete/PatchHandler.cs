namespace RDEditorPlus.Patch.CustomMethod.Autocomplete
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsAutocomplete != PluginConfig.AutocompleteBehaviour.Disabled;
    }
}
