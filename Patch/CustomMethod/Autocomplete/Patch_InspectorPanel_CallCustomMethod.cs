using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDLevelEditor;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.CustomMethod.Autocomplete
{
    internal static class Patch_InspectorPanel_CallCustomMethod
    {
        [HarmonyPatch(typeof(InspectorPanel_CallCustomMethod), nameof(InspectorPanel_CallCustomMethod.UpdateMethodDesc))]
        private static class UpdateMethodDesc
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchStloc(0))
                    .Emit(OpCodes.Ldloca_S, (byte)0)
                    .Emit(OpCodes.Ldloc_1)
                    .Emit(OpCodes.Ldarg_1)
                    .EmitDelegate((ref string translated, bool exists, string key) =>
                    {
                        if (exists)
                        {
                            return true;
                        }

                        if (CustomMethodStorage.Instance.TryGetDescription(key, out var description))
                        {
                            translated = description;
                            return true;
                        }

                        return false;
                    });

                cursor
                    .Emit(OpCodes.Stloc_1)
                    .GotoNext(MoveType.After, instruction => instruction.MatchCallvirt<PropertyControl_Description>(nameof(PropertyControl_Description.SetText)))
                    .EmitDelegate(() =>
                    {
                        var control = InspectorPanel_CallCustomMethod.methodDescription;
                        control.SetPropertyControlHeight(LayoutUtility.GetPreferredHeight(control.text.rectTransform), updateProperty: true);
                    });
            }
        }
    }
}
