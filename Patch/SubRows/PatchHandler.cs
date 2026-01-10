using HarmonyLib;
using RDEditorPlus.Util;

namespace RDEditorPlus.Patch.SubRows
{
    internal static class PatchHandler
    {
        public static void Patch(Harmony harmony)
        {
            if (!PluginConfig.SubRowsEnabled)
            {
                return;
            }

            PatchUtil.PatchAllFromCurrentNamespace(harmony, typeof(PatchHandler));
        }
    }
}
