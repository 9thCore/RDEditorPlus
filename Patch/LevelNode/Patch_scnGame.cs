using HarmonyLib;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor;
using RDLevelEditor;
using System.Collections;

namespace RDEditorPlus.Patch.LevelNode
{
    internal static class Patch_scnGame
    {
        [HarmonyPatch(typeof(scnGame), nameof(scnGame.StartTheGame))]
        private static class StartTheGame
        {
            private static bool Prefix(ref IEnumerator __result)
            {
                if (scnEditor.instance == null
                    || scnEditor.instance.userIsEditingAnInputField
                    || !RDEditorUtils.CheckForKeyCombo(false, false, PluginConfig.LevelNodeKeyCode))
                {
                    return true;
                }

                __result = DummyCoroutine();
                return false;
            }

            private static IEnumerator DummyCoroutine()
            {
                yield return null;
            }
        }
    }
}
