using HarmonyLib;
using RDEditorPlus.Functionality.CustomMethod.VariableAlias;
using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.Patch.CustomMethod.VariableAlias
{
    internal static class Patch_Conditional
    {
        [HarmonyPatch(typeof(Conditional), nameof(Conditional.Decode))]
        private static class Decode
        {
            private static void Prefix(Dictionary<string, object> dict)
            {
                VariableAliasManager.Instance.BeforeConditionalDeserialisation(dict);
            }
        }

        [HarmonyPatch(typeof(Conditional), nameof(Conditional.Encode))]
        private static class Encode
        {
            private static void Prefix(Conditional __instance)
            {
                VariableAliasManager.Instance.BeforeConditionalJSONConstruct(__instance);
            }

            private static void Postfix(Conditional __instance)
            {
                VariableAliasManager.Instance.RecoverConditionalData(__instance);
            }
        }
    }
}
