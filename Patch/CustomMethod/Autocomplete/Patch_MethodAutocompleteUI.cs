using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using System.Reflection;

namespace RDEditorPlus.Patch.CustomMethod.Autocomplete
{
    internal static class Patch_MethodAutocompleteUI
    {
        [HarmonyPatch(typeof(MethodAutocompleteUI), nameof(MethodAutocompleteUI.Search))]
        private static class Search
        {
            private static void ILManipulator(ILContext il)
            {
                const byte attributeLocalIndex = 25;
                const byte methodInfoLocalIndex = 24;

                MethodInfo isDev = typeof(RDBase).GetMethod($"get_{nameof(RDBase.isDev)}", BindingFlags.Public | BindingFlags.Static);
                MethodInfo additionalMethodAllow = typeof(Search).GetMethod(nameof(AdditionalMethodAllow), BindingFlags.NonPublic | BindingFlags.Static);

                ILCursor cursor = new(il);

                cursor.GotoNext(MoveType.After, instruction => instruction.MatchLdloc(attributeLocalIndex))
                    .Emit(OpCodes.Ldloc_S, methodInfoLocalIndex)
                    .Emit(OpCodes.Call, additionalMethodAllow);
            }

            private static bool AdditionalMethodAllow(ListedMethodAttribute attribute, MethodInfo methodInfo)
            {
                if (attribute != null)
                {
                    return true;
                }

                return CustomMethodStorage.Instance.IsAllowed(methodInfo);
            }
        }
    }
}
