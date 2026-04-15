using HarmonyLib;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.CustomMethod.VariableAlias;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RDEditorPlus.Patch
{
    internal static class Patch_LevelEvent_Base
    {
        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.Decode))]
        private static class Decode
        {
            private static void Postfix(LevelEvent_Base __instance, Dictionary<string, object> dict)
            {
                if (PluginConfig.SubRowsEnabled)
                {
                    SubRowStorage.Instance.DecodeEvent(__instance, dict);
                }
            }
        }

        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.Encode))]
        private static class Encode
        {
            private static void Postfix(LevelEvent_Base __instance, ref string __result)
            {
                StringBuilder builder = null;

                if (PluginConfig.SubRowsEnabled
                    && SubRowStorage.Instance.TryConstructJSONData(__instance, out string subRowData))
                {
                    (builder ??= new()).Append(subRowData);
                }

                if (PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsVariableAliasEnabled
                    && VariableAliasManager.Instance.TryConstructEventJSONData(__instance, out string aliasData))
                {
                    (builder ??= new()).Append(aliasData);
                }

                if (builder == null)
                {
                    return;
                }

                builder.Insert(0, __result);
                __result = builder.ToString();
            }
        }

        [HarmonyPatch(typeof(LevelEvent_Base), nameof(LevelEvent_Base.DecodeTargetId))]
        private static class DecodeTargetId
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchCall<Debug>(nameof(Debug.LogWarning)))
                    .Next.Operand = AccessTools.Method(typeof(DecodeTargetId), nameof(ConditionalWarn));
            }

            private static void ConditionalWarn(string text)
            {
                if (!LevelUtil.DisableTargetIDWarning)
                {
                    Debug.LogWarning(text);
                }
            }
        }
    }
}
