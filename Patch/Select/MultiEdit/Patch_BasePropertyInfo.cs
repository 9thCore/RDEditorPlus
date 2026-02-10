using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Linq;
using System.Reflection;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_BasePropertyInfo
    {
        [HarmonyPatch(typeof(BasePropertyInfo), MethodType.Constructor)]
        [HarmonyPatch( [typeof(PropertyInfo)] )]
        private static class Constructor
        {
            private static void Postfix(BasePropertyInfo __instance)
            {
                if (__instance.enableIf != null)
                {
                    Func<LevelEvent_Base, bool> enableIf = __instance.enableIf;

                    typeof(BasePropertyInfo)
                        .GetField(nameof(BasePropertyInfo.enableIf))
                        .SetValue(__instance,
                        (LevelEvent_Base levelEvent) =>
                        {
                            if (!enableIf(levelEvent))
                            {
                                return false;
                            }

                            if (!InspectorUtil.CanMultiEdit())
                            {
                                return true;
                            }

                            return scnEditor.instance.selectedControls.All(ec => enableIf(ec.levelEvent));
                        });
                }
            }
        }
    }
}
