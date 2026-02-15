using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_InspectorPanel_SetRowXs
    {
        [HarmonyPatch(typeof(InspectorPanel_SetRowXs), nameof(InspectorPanel_SetRowXs.SaveProperties))]
        private static class SaveProperties
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(instruction => instruction.MatchLdstr("pattern"))
                    .GotoNext(MoveType.Before, instruction => instruction.MatchBrfalse(out _))
                    .EmitDelegate(() => PropertyStorage.Instance.beatModifierSyncoChanged);

                cursor
                    .Emit(OpCodes.And);
            }
        }
    }
}
