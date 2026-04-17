using HarmonyLib;
using RDEditorPlus.Functionality.CustomMethod.VariableDisplay;
using RDLevelEditor;
using System.Text;

namespace RDEditorPlus.Patch.CustomMethod.VariableDisplay
{
    internal static class Patch_RDInspectorPanelManager
    {
        [HarmonyPatch(typeof(RDInspectorPanelManager), nameof(RDInspectorPanelManager.GetRDCodeVariables))]
        private static class GetRDCodeVariables
        {
            private static bool Prefix(RDInspectorPanelManager __instance, ref string __result)
            {
                if (__instance.game == null)
                {
                    __result = string.Empty;
                    return false;
                }

                if (!VariableDisplayManager.Instance.HasExpressions())
                {
                    return true;
                }

                StringBuilder builder = new();

                LevelBase currentLevel = __instance.game.currentLevel;

                foreach (var expression in VariableDisplayManager.Instance.Expressions)
                {
                    if (expression.TryEvaluate(currentLevel, out var result) && ShouldPrint(result))
                    {
                        builder.AppendLine($"{expression.Original}: {result}");
                    }
                }

                __result = builder.ToString();
                return false;
            }
        }

        private static bool ShouldPrint(object value)
        {
            if (value is int intValue)
            {
                return intValue != 0;
            }
            else if (value is float floatValue)
            {
                return floatValue != 0f;
            }
            else if (value is double doubleValue)
            {
                return doubleValue != 0d;
            }
            else if (value is bool boolValue)
            {
                return boolValue;
            }

            return true;
        }
    }
}
