using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Linq;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_InspectorPanel_CallCustomMethod
    {
        [HarmonyPatch(typeof(InspectorPanel_CallCustomMethod), nameof(InspectorPanel_CallCustomMethod.CheckMethodDesc))]
        private static class CheckMethodDesc
        {
            private static bool Prefix()
            {
                if (!InspectorUtil.CanMultiEdit()
                    || methodInput.EqualValueForSelectedEvents())
                {
                    return true;
                }

                InspectorPanel_CallCustomMethod.ClearMethodDesc();
                return false;
            }
        }

        [HarmonyPatch(typeof(InspectorPanel_CallCustomMethod), nameof(InspectorPanel_CallCustomMethod.Start))]
        private static class Start
        {
            private static void Postfix(InspectorPanel_CallCustomMethod __instance)
            {
                methodInput = __instance.properties.Where(property => property.name == nameof(LevelEvent_CallCustomMethod.methodName))
                    .Select(property => property.control as PropertyControl_InputField)
                    .First();
            }
        }

        private static PropertyControl_InputField methodInput;
    }
}
