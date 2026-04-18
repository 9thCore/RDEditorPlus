using HarmonyLib;
using RDEditorPlus.Functionality.CustomMethod.VariableDisplay;
using RDEditorPlus.Functionality.General;

namespace RDEditorPlus.Patch.CustomMethod.VariableDisplay
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsVariableDisplayEnabled;

        public override void Patch(Harmony harmony)
        {
            base.Patch(harmony);

            SettingsInspectorRegistry.Register(
                new SettingsInspectorRegistry.ButtonOption("Display", () => VariableDisplayPanelHolder.Instance.Toggle(show: true)));
        }
    }
}
