using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_InspectorPanel_SetVFXPreset
    {
        [HarmonyPatch(typeof(InspectorPanel_SetVFXPreset), nameof(InspectorPanel_SetVFXPreset.UpdateUIProperties))]
        private static class UpdateUIProperties
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor.GotoNext(
                    MoveType.Before,
                    instruction => instruction.MatchLdloc(0),
                    instruction => instruction.MatchCallvirt<LevelEvent_SetVFXPreset>(nameof(LevelEvent_SetVFXPreset.UpdateRoomsUsage)));

                var label = cursor.MarkLabel();

                // Just skip the entire thing aside from the room usage method lol
                cursor
                    .GotoPrev(instruction => instruction.MatchLdstr("preset"))
                    .GotoNext(MoveType.Before, instruction => instruction.MatchLdloc(2))
                    .Emit(OpCodes.Br, label);
            }
        }
    }
}
