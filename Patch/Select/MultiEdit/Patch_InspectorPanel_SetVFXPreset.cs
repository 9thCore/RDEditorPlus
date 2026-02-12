using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_InspectorPanel_SetVFXPreset
    {
        [HarmonyPatch(typeof(InspectorPanel_SetVFXPreset), nameof(InspectorPanel_SetVFXPreset.UpdateUIProperties))]
        private static class UpdateUIProperties
        {
            private static void ILManipulator(ILContext il)
            {
                const int bloomVFXIndex = (int)RDThemeFX.Bloom;
                const byte inputFieldPropertyLocalIndex = 8;

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

                ILLabel label2 = null;
                cursor
                    .GotoNext(instruction => instruction.MatchLdcI4(bloomVFXIndex))
                    .GotoNext(instruction => instruction.MatchBneUn(out label2))
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStloc(inputFieldPropertyLocalIndex))
                    .EmitDelegate((PropertyControl_InputField inputField) =>
                    {
                        inputField.parentProperty.UpdateUI(scnEditor.instance.selectedControls[0].levelEvent);
                    });

                cursor
                    .Emit(OpCodes.Br, label2);
            }
        }

        [HarmonyPatch(typeof(InspectorPanel_SetVFXPreset), nameof(InspectorPanel_SetVFXPreset.SaveProperties))]
        private static class SaveProperties
        {
            private static void ILManipulator(ILContext il)
            {
                const int bloomVFXIndex = (int)RDThemeFX.Bloom;
                const byte inputFieldPropertyLocalIndex = 7;

                ILCursor cursor = new(il);

                ILLabel skipLabel = null;
                cursor
                    .GotoNext(instruction => instruction.MatchLdcI4(bloomVFXIndex))
                    .GotoNext(instruction => instruction.MatchBneUn(out skipLabel))
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStloc(inputFieldPropertyLocalIndex))
                    .EmitDelegate((PropertyControl_InputField inputField) =>
                    {
                        if (float.TryParse(inputField.inputField.text, out float value))
                        {
                            float clamped = Mathf.Clamp(value, -5f, 5f);
                            inputField.inputField.text = value.ToString();

                            foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                            {
                                if (eventControl.levelEvent is LevelEvent_SetVFXPreset vfxEvent)
                                {
                                    vfxEvent.intensity = vfxEvent.preset == RDThemeFX.Bloom ? clamped : value;
                                }
                            }
                        }
                        else
                        {
                            foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                            {
                                if (eventControl.levelEvent is LevelEvent_SetVFXPreset vfxEvent
                                && vfxEvent.preset == RDThemeFX.Bloom)
                                {
                                    vfxEvent.intensity = Mathf.Clamp(vfxEvent.intensity, -5f, 5f);
                                }
                            }
                        }

                        inputField.UpdateUI(scnEditor.instance.selectedControls[0].levelEvent);
                    });

                cursor
                    .Emit(OpCodes.Br, skipLabel);
            }
        }
    }
}
