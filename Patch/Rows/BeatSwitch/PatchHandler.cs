namespace RDEditorPlus.Patch.Rows.BeatSwitch
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.RowsEnabled && PluginConfig.RowBeatSwitch != PluginConfig.RowBeatSwitchBehaviour.Disabled;
    }
}
