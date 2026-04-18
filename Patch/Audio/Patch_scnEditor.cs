using HarmonyLib;
using RDEditorPlus.Functionality.Audio;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Audio
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Awake))]
        private static class Awake
        {
            private static void Postfix()
            {
                AudioOneTrueNameAutocompletion.Instance.StartFetch();
            }
        }
    }
}
