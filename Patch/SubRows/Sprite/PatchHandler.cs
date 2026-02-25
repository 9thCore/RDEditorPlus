namespace RDEditorPlus.Patch.SubRows.Sprite
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.SubRowsEnabled && PluginConfig.SpriteSubRowsEnabled;
    }
}
