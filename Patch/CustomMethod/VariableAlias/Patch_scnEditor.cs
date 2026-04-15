using HarmonyLib;
using RDEditorPlus.Functionality.CustomMethod.VariableAlias;
using RDLevelEditor;

namespace RDEditorPlus.Patch.CustomMethod.VariableAlias
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.EncodeData))]
        private static class EncodeData
        {
            private static void Prefix()
            {
                VariableAliasManager.Instance.ClearOriginalValues();
            }
        }
    }
}
