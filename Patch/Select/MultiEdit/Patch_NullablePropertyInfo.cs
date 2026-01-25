using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_NullablePropertyInfo
    {
        [HarmonyPatch(typeof(NullablePropertyInfo), nameof(NullablePropertyInfo.IsUsed))]
        private static class IsUsed
        {
            private static bool Prefix(NullablePropertyInfo __instance, ref bool __result)
            {
                if (!InspectorUtil.CanMultiEdit())
                {
                    return true;
                }

                __result = __instance.IsUsedMultiEdit();
                return false;
            }
        }
    }
}
