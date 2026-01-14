using HarmonyLib;
using RDEditorPlus.Functionality.SubRow;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_LevelEventControl_Base
    {
        [HarmonyPatch(typeof(LevelEventControl_Base), nameof(LevelEventControl_Base.SetPosWithSpriteTarget))]
        private static class SetPosWithSpriteTarget
        {
            private static void Postfix(LevelEventControl_Base __instance)
            {
                SpriteManager.Instance.UpdateUI(__instance);
            }
        }
    }
}
