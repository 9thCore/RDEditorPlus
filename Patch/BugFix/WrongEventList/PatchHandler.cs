namespace RDEditorPlus.Patch.BugFix.WrongEventList
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.BugFixesEnabled && PluginConfig.BugFixesWrongEventListEnabled;
    }
}
