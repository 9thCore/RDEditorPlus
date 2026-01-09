using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;

namespace RDEditorPlus.Patch
{
    [HarmonyPatch]
    internal class Patch_LevelEventControl_Base
    {
        [HarmonyPatch(typeof(LevelEventControl_Base), nameof(LevelEventControl_Base.SetPosWithSpriteTarget))]
        private static class SetPosWithSpriteTarget
        {
            private static void Postfix(LevelEventControl_Base __instance)
            {
                SubRowStorage.Holder.OffsetLevelEventPosition(__instance);
            }
        }
    }
}
