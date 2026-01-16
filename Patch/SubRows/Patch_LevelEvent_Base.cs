using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class Patch_LevelEvent_Base
    {
        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.CopyBasePropertiesFrom))]
        private static class CopyBasePropertiesFrom
        {
            private static void Postfix(LevelEvent_Base __instance, LevelEvent_Base levelEvent)
            {
                SubRowStorage.Instance.CopyData(source: levelEvent, destination: __instance);
            }
        }
    }
}
