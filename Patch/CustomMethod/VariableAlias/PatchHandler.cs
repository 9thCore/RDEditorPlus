using HarmonyLib;
using RDEditorPlus.Functionality.CustomMethod.VariableAlias;
using RDEditorPlus.Functionality.General;

namespace RDEditorPlus.Patch.CustomMethod.VariableAlias
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsVariableAliasEnabled;

        public override void Patch(Harmony harmony)
        {
            base.Patch(harmony);

            SettingsInspectorRegistry.Register(
                new SettingsInspectorRegistry.ButtonOption("Aliases", () => VariableAliasPanelHolder.Instance.Toggle(show: true)));
        }
    }
}
