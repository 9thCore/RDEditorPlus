using HarmonyLib;
using RDEditorPlus.Functionality.CustomMethod;
using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.Patch.CustomMethod.VariableAlias
{
    internal static class Patch_LevelEvent_Base
    {
        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.Decode))]
        private static class Decode
        {
            private static void Prefix(LevelEvent_Base __instance, Dictionary<string, object> dict)
            {
                if (!__instance.editor.changingFile)
                {
                    return;
                }

                VariableAliasManager.Instance.BeforeEventDeserialisation(dict);
            }
        }

        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.Encode))]
        private static class Encode
        {
            private static void Prefix(LevelEvent_Base __instance, ref bool __state)
            {
                if (__instance.editor.savingState)
                {
                    __state = false;
                    return;
                }

                __state = true;
                VariableAliasManager.Instance.BeforeEventJSONConstruct(__instance);
            }

            private static void Postfix(LevelEvent_Base __instance, bool __state)
            {
                if (!__state)
                {
                    return;
                }

                VariableAliasManager.Instance.RecoverEventData(__instance);
            }
        }
    }
}
