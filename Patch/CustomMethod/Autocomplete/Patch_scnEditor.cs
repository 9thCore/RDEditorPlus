using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;

namespace RDEditorPlus.Patch.CustomMethod.Autocomplete
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Awake))]
        private static class Awake
        {
            private static void Postfix()
            {
                CustomMethodStorage.Instance.StartFetchOfMethods();
            }
        }
    }
}
