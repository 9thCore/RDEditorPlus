namespace RDEditorPlus.Patch.LevelOptions.CustomClass
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.LevelOptionsEnabled && PluginConfig.LevelOptionsCustomClassEnabled;
    }
}
