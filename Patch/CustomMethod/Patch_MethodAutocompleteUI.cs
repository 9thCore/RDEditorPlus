using HarmonyLib;
using RDEditorPlus.ExtraData;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RDEditorPlus.Patch.CustomMethod
{
    internal static class Patch_MethodAutocompleteUI
    {
        [HarmonyPatch(typeof(MethodAutocompleteUI), nameof(MethodAutocompleteUI.Search))]
        private static class Search
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                if (!PluginConfig.CustomMethodsAutocomplete)
                {
                    return instructions;
                }

                return new CodeMatcher(instructions)
                    // No-op the first guard clause (dev check)
                    .MatchForward(false, new CodeMatch(OpCodes.Ret))
                    .Set(OpCodes.Nop, null)

                    .InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(MethodAutocompleteUI), nameof(MethodAutocompleteUI.StringifyMethod))]
        private static class StringifyMethod
        {
            private static bool Prefix(MethodInfo methodInfo, ref bool __result)
            {
                if (!RDBase.isDev
                    && !CustomMethodStorage.Instance.IsAllowed(methodInfo))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}
