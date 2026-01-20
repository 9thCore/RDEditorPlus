namespace RDEditorPlus.Patch.CustomMethod
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.CustomMethodsEnabled;
    }
}
