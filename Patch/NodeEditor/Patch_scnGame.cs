using HarmonyLib;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor;
using System.Collections;

namespace RDEditorPlus.Patch.NodeEditor
{
    internal static class Patch_scnGame
    {
        [HarmonyPatch(typeof(scnGame), nameof(scnGame.StartTheGame))]
        private static class StartTheGame
        {
            private static bool Prefix(ref IEnumerator __result)
            {
                if (NodePanelHolder.CurrentPanel == null)
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
