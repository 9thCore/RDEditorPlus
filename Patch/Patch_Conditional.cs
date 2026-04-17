using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.CustomMethod.VariableAlias;
using RDLevelEditor;
using System.Text;

namespace RDEditorPlus.Patch
{
    internal static class Patch_Conditional
    {
        [HarmonyPatch(typeof(Conditional), nameof(Conditional.Encode))]
        private static class Encode
        {
            private static void Postfix(Conditional __instance, ref string __result)
            {
                StringBuilder builder = null;

                if (PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsVariableAliasEnabled
                    && VariableAliasManager.Instance.TryConstructConditionalJSONData(__instance, out string aliasData))
                {
                    (builder ??= new()).Append(aliasData);
                }

                if (builder == null)
                {
                    return;
                }

                if (builder[0] == ',')
                {
                    builder.Remove(0, builder[1] == ' ' ? 2 : 1);
                }

                builder.Insert(0, __result);
                __result = builder.ToString();
            }
        }
    }
}
