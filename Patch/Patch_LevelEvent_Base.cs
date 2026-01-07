using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;
using System.Collections.Generic;
using System.Text;

namespace RDEditorPlus.Patch
{
    [HarmonyPatch]
    internal static class Patch_LevelEvent_Base
    {
        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.Decode))]
        private static class Decode
        {
            private static void Postfix(LevelEvent_Base __instance, Dictionary<string, object> dict)
            {
                SubRowStorage.Holder.DecodeEvent(__instance, dict);
            }
        }

        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.Encode))]
        private static class Encode
        {
            private static void Postfix(LevelEvent_Base __instance, ref string __result)
            {
                StringBuilder builder = null;

                if (SubRowStorage.Holder.TryConstructJSONData(__instance, out string subRowData))
                {
                    (builder ??= new()).Append(subRowData);
                }

                if (builder == null)
                {
                    return;
                }

                builder.Insert(0, __result);
                __result = builder.ToString();
            }
        }
    }
}
