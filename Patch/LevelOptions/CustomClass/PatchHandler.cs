using HarmonyLib;
using RDEditorPlus.Functionality.LevelOptions.CustomClass;

namespace RDEditorPlus.Patch.LevelOptions.CustomClass
{
    internal class PatchHandler : BasePatchHandler<PatchHandler>
    {
        protected override bool CanApply => PluginConfig.LevelOptionsEnabled && PluginConfig.LevelOptionsCustomClassEnabled;

        public override void Patch(Harmony harmony)
        {
            base.Patch(harmony);

            CustomClassDropdownFunctionality.Register();
        }
    }
}
