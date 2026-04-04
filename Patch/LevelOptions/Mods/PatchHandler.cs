namespace RDEditorPlus.Patch.LevelOptions.Mods
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.LevelOptionsEnabled && PluginConfig.LevelOptionsModsEnabled;
    }
}
