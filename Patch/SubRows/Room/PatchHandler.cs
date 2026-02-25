namespace RDEditorPlus.Patch.SubRows.Room
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.SubRowsEnabled && PluginConfig.RoomSubRowsEnabled;
    }
}
