using HarmonyLib;
using MonoMod.Cil;
using RDEditorPlus.Functionality.CustomMethod.VariableAlias;
using RDEditorPlus.Functionality.Windows;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDEditorPlus.Patch
{
    internal static class Patch_RDLevelData
    {
        [HarmonyPatch(typeof(RDLevelData), MethodType.Constructor, [typeof(Dictionary<string, object>), typeof(bool), typeof(bool), typeof(bool)])]
        private static class ctor
        {
            private static void Postfix(Dictionary<string, object> rootDict)
            {
                if (scnEditor.instance == null
                    || !scnEditor.instance.changingFile
                    || RDLevelData.decodingFailed)
                {
                    return;
                }

                Dictionary<string, object> dict = null;
                if (rootDict.TryGetValue(ModDataKey, out var value))
                {
                    dict = value as Dictionary<string, object>;
                }

                if (PluginConfig.WindowsEnabled && PluginConfig.WindowsMoreEnabled)
                {
                    MoreWindowManager.Instance.DecodeModData(dict);
                }

                if (PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsVariableAliasEnabled)
                {
                    VariableAliasManager.Instance.DecodeModData(dict);
                }
            }
        }

        [HarmonyPatch(typeof(RDLevelData), nameof(RDLevelData.Encode))]
        private static class Encode
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(instruction => instruction.MatchLdstr("settings"))
                    .GotoPrev(MoveType.Before, instruction => instruction.MatchLdsfld<RDLevelData>(nameof(RDLevelData.encode)))
                    .EmitDelegate(ApplyJSONData);
            }

            private static void ApplyJSONData()
            {
                if (scnEditor.instance == null)
                {
                    return;
                }

                StringBuilder builder = null;

                BuildData(ref builder, PluginConfig.WindowsEnabled && PluginConfig.WindowsMoreEnabled,
                    MoreWindowManager.Instance.TryConstructJSONData);

                BuildData(ref builder, PluginConfig.CustomMethodsEnabled && PluginConfig.CustomMethodsVariableAliasEnabled,
                    VariableAliasManager.Instance.TryConstructJSONData);

                if (builder == null)
                {
                    return;
                }

                RDLevelData.encode.Append(RDEditorUtils.OpenDictionary(ModDataKey, isFirstMember: false));
                RDLevelData.encode.Append(builder.ToString());
                RDLevelData.encode.Append(Environment.NewLine);
                RDLevelData.encode.Append(RDEditorUtils.CloseDictionary(isLastMember: false));
            }
        }

        private static void BuildData(ref StringBuilder builder, bool guard, DataFetch dataFetch)
        {
            if (!guard || !dataFetch(out string data))
            {
                return;
            }

            if (builder == null)
            {
                builder = new(Indentation);
            }
            else
            {
                builder.Append(",");
                builder.Append(Environment.NewLine);
                builder.Append(Indentation);
            }

            builder.Append(data);
        }

        private const string ModDataKey = "mod_rdEditorPlus";
        private const string Indentation = "\t\t";

        private delegate bool DataFetch(out string result);
    }
}
