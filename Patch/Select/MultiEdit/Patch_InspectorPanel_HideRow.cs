using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDLevelEditor;
using System.Linq;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_InspectorPanel_HideRow
    {
        [HarmonyPatch(typeof(InspectorPanel_HideRow), nameof(InspectorPanel_HideRow.UpdateUIProperties))]
        private static class UpdateUIProperties
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchCallvirt<LevelEvent_HideRow>($"get_{nameof(LevelEvent_HideRow.show)}"))
                    .EmitDelegate((RowVisibilityMode _) =>
                    {
                        return scnEditor.instance.selectedControls
                        .Select(control => control.levelEvent as LevelEvent_HideRow)
                        .Any(levelEvent => levelEvent != null && levelEvent.show != RowVisibilityMode.Visible);
                    });
            }
        }
    }
}
