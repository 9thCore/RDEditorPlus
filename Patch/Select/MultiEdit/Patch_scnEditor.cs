using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Start))]
        private static class Start
        {
            private static void Postfix()
            {
                EventUtil.UpdateVFXPresetDropdown();
                PropertyStorage.Instance.UpdateRowPropertyControls();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DecodeData))]
        private static class DecodeData
        {
            private static void Postfix()
            {
                EventUtil.UpdateVFXPresetDropdown();
                PropertyStorage.Instance.UpdateRowPropertyControls();
            }
        }

        [HarmonyPatch(typeof(scnEditor), $"get_{nameof(scnEditor.selectedControl)}")]
        private static class getSelectedControl
        {
            private static bool Prefix(scnEditor __instance, ref LevelEventControl_Base __result)
            {
                if (__instance.selectedControls.Count == 0)
                {
                    __result = null;
                    return false;
                }

                __result = __instance.selectedControls[0];
                return false;
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.InspectorPanel_UpdateUI))]
        private static class InspectorPanel_UpdateUI
        {
            private static bool Prefix()
            {
                if (!InspectorUtil.CanMultiEdit())
                {
                    return true;
                }

                var control = scnEditor.instance.selectedControls[0];
                var panel = control.levelEvent.inspectorPanel;

                panel.UpdateUI(control.levelEvent);
                panel.position.MultiEditUpdateUI();

                return false;
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.AddNewRow))]
        private static class AddNewRow
        {
            private static void Postfix()
            {
                PropertyStorage.Instance.ScheduleRowPropertyControlsUpdate();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DeleteRowClick))]
        private static class DeleteRowClick
        {
            private static void Postfix()
            {
                PropertyStorage.Instance.UpdateRowPropertyControls();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SwapRowPositions))]
        private static class SwapRowPositions
        {
            private static void Postfix()
            {
                PropertyStorage.Instance.ScheduleRowPropertyControlsUpdate();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SetLevelEventControlType))]
        private static class SetLevelEventControlType
        {
            private static bool Prefix(LevelEventType levelEventType, bool copyRow)
            {
                var editor = scnEditor.instance;

                if (editor.filterMode
                    || editor.selectedControls.Count == 1)
                {
                    return true;
                }

                Type type = GameAssembly.GetType($"RDLevelEditor.LevelEvent_{levelEventType}");

                List<LevelEventControl_Base> newControls = new(editor.selectedControls.Count);
                List<LevelEventControl_Base> controlListCopy = editor.selectedControls.ToList();

                foreach (var control in controlListCopy)
                {
                    LevelEvent_Base newLevelEvent = (LevelEvent_Base)Activator.CreateInstance(type);
                    newLevelEvent.CopyBasePropertiesFrom(control.levelEvent, copyBarAndBeat: true, copyRow);
                    newLevelEvent.OnCreate();

                    Tab tab = control.tab;
                    if (tab == Tab.Sprites && newLevelEvent.type != LevelEventType.None)
                    {
                        LevelEvent_MakeSprite levelEvent_MakeSprite = editor.currentPageSpritesData[newLevelEvent.y];
                        newLevelEvent.target = levelEvent_MakeSprite.spriteId;
                    }

                    editor.DeleteEventControl(control, false, false);
                    LevelEventControl_Base newControl = editor.CreateEventControl(newLevelEvent, tab, false);
                    newControl.UpdateUI();

                    newControls.Add(newControl);
                }

                editor.UpdateTimelineAccordingToLevelEventType(levelEventType);
                editor.SelectEventControls(newControls);

                return false;
            }

            private static readonly Assembly GameAssembly = typeof(scnEditor).Assembly;
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.AddEventControlToSelection))]
        private static class AddEventControlToSelection
        {
            private static void Postfix(scnEditor __instance)
            {
                if (__instance.conditionalsPanel.showingListPanel)
                {
                    __instance.conditionalsPanel.ShowListPanel(visible: true);
                }
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SelectEventControl))]
        private static class SelectEventControl
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(instruction => instruction.MatchCall<UnityEngine.Object>("op_Equality"))
                    .GotoNext(MoveType.Before, instruction => instruction.MatchBrfalse(out _))
                    .EmitDelegate(PropertyStorage.Instance.GetNotForceSelectEvent);

                cursor
                    .Emit(OpCodes.And);
            }
        }
    }
}
