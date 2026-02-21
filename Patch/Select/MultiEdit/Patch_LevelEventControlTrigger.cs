using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_LevelEventControlEventTrigger
    {
        [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.UpdateInspectorPanelIfNecessary))]
        private static class UpdateInspectorPanelIfNecessary
        {
            private static void Postfix()
            {
                if (scnEditor.instance.selectedControls.Count > 1)
                {
                    LevelEvent_Base levelEvent = scnEditor.instance.selectedControls[0].levelEvent;

                    if (!levelEvent.isBaseEvent)
                    {
                        levelEvent.inspectorPanel.UpdateUI(levelEvent);
                    }
                }
            }
        }

        private static class DragFromRight
        {
            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragClassicFromRight))]
            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragOneshotFromRight))]
            private static void Prefix(LevelEventControlEventTrigger __instance)
            {
                if (!float.IsNaN(dragDelta))
                {
                    return;
                }

                float snappedPosition = __instance.SnapXPositionToGrid(__instance.XPositionInsideCell);
                LevelEventControl_AddClassicBeat eventControl = __instance.control as LevelEventControl_AddClassicBeat;

                dragDelta = snappedPosition - eventControl.hitPulse.anchoredPosition.x;
            }

            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragClassicFromRight))]
            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragOneshotFromRight))]
            private static void Postfix(LevelEventControlEventTrigger __instance, bool playSound)
            {
                if (callingMethodForEveryoneElse)
                {
                    return;
                }

                callingMethodForEveryoneElse = true;

                foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                {
                    if (__instance.control != eventControl)
                    {
                        if (eventControl.levelEvent.type == LevelEventType.AddClassicBeat)
                        {
                            eventControl.trigger.DragClassicFromRight(playSound);
                        }
                        else if (eventControl.levelEvent.type == LevelEventType.AddOneshotBeat)
                        {
                            eventControl.trigger.DragOneshotFromRight(playSound);
                        }
                    }
                }

                callingMethodForEveryoneElse = false;
                dragDelta = float.NaN;
            }

            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragClassicFromRight))]
            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragOneshotFromRight))]
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchCall(
                        typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.SnapXPositionToGrid)))
                    .Emit(OpCodes.Ldloc_0)
                    .EmitDelegate((float mousePosition, LevelEventControl_AddClassicBeat eventControl) =>
                    {
                        if (callingMethodForEveryoneElse)
                        {
                            return eventControl.hitPulse.anchoredPosition.x + dragDelta;
                        }

                        return mousePosition;
                    });
            }

            private static float dragDelta = float.NaN;
            private static bool callingMethodForEveryoneElse = false;
        }

        private static class DragFromLeft
        {
            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragClassicFromLeft))]
            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragOneshotFromLeft))]
            private static void Prefix(LevelEventControlEventTrigger __instance)
            {
                if (!float.IsNaN(dragDelta))
                {
                    return;
                }

                float snappedPosition = Mathf.Max(0f, __instance.SnapXPositionToGrid(__instance.timeline.MousePositionOnTimeline.x));
                LevelEventControl_AddClassicBeat eventControl = __instance.control as LevelEventControl_AddClassicBeat;

                dragDelta = snappedPosition - eventControl.rt.anchoredPosition.x;
            }

            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragClassicFromLeft))]
            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragOneshotFromLeft))]
            private static void Postfix(LevelEventControlEventTrigger __instance, bool playSound)
            {
                if (callingMethodForEveryoneElse)
                {
                    return;
                }

                callingMethodForEveryoneElse = true;

                foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                {
                    if (__instance.control != eventControl)
                    {
                        if (eventControl.levelEvent.type == LevelEventType.AddClassicBeat)
                        {
                            eventControl.trigger.DragClassicFromLeft(playSound);
                        }
                        else if (eventControl.levelEvent.type == LevelEventType.AddOneshotBeat)
                        {
                            eventControl.trigger.DragOneshotFromLeft(playSound);
                        }
                    }
                }

                callingMethodForEveryoneElse = false;
                dragDelta = float.NaN;
            }

            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragClassicFromLeft))]
            [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.DragOneshotFromLeft))]
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(instruction => instruction.MatchCall(typeof(Mathf), nameof(Mathf.Max)))
                    .GotoPrev(MoveType.Before, instruction => instruction.MatchCall(
                        typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.SnapXPositionToGrid)))
                    .Emit(OpCodes.Ldloc_0)
                    .EmitDelegate((float mousePosition, LevelEventControl_AddClassicBeat eventControl) =>
                    {
                        if (callingMethodForEveryoneElse)
                        {
                            return eventControl.rt.anchoredPosition.x + dragDelta;
                        }

                        return mousePosition;
                    });
            }

            private static float dragDelta = float.NaN;
            private static bool callingMethodForEveryoneElse = false;
        }

        [HarmonyPatch(typeof(LevelEventControlEventTrigger), nameof(LevelEventControlEventTrigger.OnPointerClick))]
        private static class OnPointerClick
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchStsfld<LevelEventControlEventTrigger>(nameof(LevelEventControlEventTrigger.eventControls)))
                    .EmitDelegate(PropertyStorage.Instance.ForceSelectEvent);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchStloc(19))
                    .EmitDelegate(PropertyStorage.Instance.ForceSelectEvent);
            }
        }
    }
}
