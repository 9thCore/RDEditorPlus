using HarmonyLib;
using RDEditorPlus.Functionality.MultiEdit;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_Conditionals
    {
        [HarmonyPatch(typeof(Conditionals), nameof(Conditionals.ShowListPanel))]
        private static class ShowListPanel
        {
            private static void Postfix(Conditionals __instance, bool visible)
            {
                if (!visible || !InspectorUtil.CanMultiEdit())
                {
                    return;
                }

                MultiEditManager.Instance.ApplyConditionalButtonThings(__instance);
            }
        }
    }
}
