using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_Row
    {
        [HarmonyPatch(typeof(PropertyControl_Row), nameof(PropertyControl_Row.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_Row __instance)
            {
                CustomDropdown customDropdown = __instance.dropdown.ReplaceWithDerivative<CustomDropdown>();
                __instance.dropdown = customDropdown;

                PropertyStorage.Instance.AddRowPropertyControl(__instance);
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Row), nameof(PropertyControl_Row.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_Row __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    __instance.dropdown.captionText.text = InspectorUtil.MixedText;
                    return false;
                }

                __instance.dropdown.RefreshShownValue();
                return true;
            }

            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchCall(typeof(Convert), nameof(Convert.ToInt32)));

                var label = cursor.MarkLabel();

                cursor
                    .GotoPrev(MoveType.After, instruction => instruction.MatchCall<PropertyControl>(nameof(PropertyControl.GetEventValue)))
                    .Emit(OpCodes.Br, label);
            }
        }
    }
}
