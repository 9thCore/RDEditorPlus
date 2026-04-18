namespace RDEditorPlus.Patch.Audio
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.AudioEnabled
            && PluginConfig.AudioOTNAutocompleteBehaviour != PluginConfig.AutocompleteBehaviour.Disabled;
    }
}
