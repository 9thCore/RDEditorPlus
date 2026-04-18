using HarmonyLib;
using RDEditorPlus.Functionality.General;
using RDEditorPlus.Functionality.LevelOptions.Mods;

namespace RDEditorPlus.Patch.LevelOptions.Mods
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.LevelOptionsEnabled && PluginConfig.LevelOptionsModsEnabled;

        public override void Patch(Harmony harmony)
        {
            base.Patch(harmony);

            SettingsInspectorRegistry.Register(
                new SettingsInspectorRegistry.ButtonOption("Mods", () => ModPanelHolder.Instance.Toggle(show: true)));
        }
    }
}
