using HarmonyLib;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDEditorPlus.Patch
{
    internal static class Patch_RDLevelData
    {
        [HarmonyPatch(typeof(RDLevelData), MethodType.Constructor, [typeof(Dictionary<string, object>), typeof(bool), typeof(bool)])]
        private static class ctor
        {
            private static void Postfix(Dictionary<string, object> rootDict)
            {
                if (RDLevelData.decodingFailed)
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
                StringBuilder builder = null;

                if (PluginConfig.WindowsEnabled && PluginConfig.WindowsMoreEnabled
                    && MoreWindowManager.Instance.TryConstructJSONData(out string moreWindowData))
                {
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

                    builder.Append(moreWindowData);
                }

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

        private const string ModDataKey = "mod_rdEditorPlus";
        private const string Indentation = "\t\t";
    }
}
