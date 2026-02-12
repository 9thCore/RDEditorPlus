using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_RDRoomsSelectionPopup
    {
        [HarmonyPatch(typeof(RDRoomsSelectionPopup), nameof(RDRoomsSelectionPopup.ToggleClicked))]
        private static class ToggleClicked
        {
            private static bool Prefix(RDRoomsSelectionPopup __instance, int index)
            {
                RoomUtil.Usage[] eventUsage = RoomUtil.GetSelectedEventsUsage();
                bool intendingToDisable = eventUsage[index] == RoomUtil.Usage.UsedByAll;

                var controls = scnEditor.instance.selectedControls;
                foreach (var control in controls)
                {
                    RoomsUsage usage = control.levelEvent.roomsUsage;

                    if (intendingToDisable)
                    {
                        if ((usage == RoomsUsage.ManyRooms || usage == RoomsUsage.ManyRoomsAndOnTop)
                            && control.levelEvent.rooms.Length > 1)
                        {
                            control.levelEvent.rooms = control.levelEvent.rooms.Where(room => room != index).ToArray();
                        }
                    }
                    else
                    {
                        if (index == 4
                            || usage == RoomsUsage.OneRoom
                            || usage == RoomsUsage.OneRoomOrOnTop
                            || Array.IndexOf(control.levelEvent.rooms, 4) != -1)
                        {
                            control.levelEvent.rooms = [index];
                        }
                        else if (Array.IndexOf(control.levelEvent.rooms, index) == -1)
                        {
                            Array.Resize(ref control.levelEvent.rooms, control.levelEvent.rooms.Length + 1);
                            control.levelEvent.rooms[control.levelEvent.rooms.Length - 1] = index;
                        }
                    }
                }

                if (controls.Count == 1)
                {
                    controls[0].levelEvent.inspectorPanel.position.rooms = controls[0].levelEvent.rooms;
                }

                scnEditor.instance.selectedControls[0].levelEvent.inspectorPanel.position.UpdateUI();
                return false;
            }

            private static void ILManipulator(ILContext il, ILLabel retLabel)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchStfld<LevelEvent_Base>(nameof(LevelEvent_Base.rooms)))
                    .Emit(OpCodes.Br, retLabel);
            }
        }

        [HarmonyPatch(typeof(RDRoomsSelectionPopup), nameof(RDRoomsSelectionPopup.UpdateUI))]
        private static class UpdateUI
        {
            private static void Prefix(RDRoomsSelectionPopup __instance)
            {
                if (toggleContainer == null)
                {
                    toggleContainer = __instance.toggles[0].transform.parent.gameObject;
                }

                if (applyTo == null)
                {
                    applyTo = __instance.GetComponentInChildren<Text>().gameObject;

                    mismatch = GameObject.Instantiate(applyTo);
                    mismatch.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_MismatchText";
                    mismatch.transform.parent = applyTo.transform.parent;
                    mismatch.transform.localScale = Vector3.one;
                    mismatch.SetActive(false);

                    mismatch.GetComponent<Text>().text = "Room usage mode mismatch, can't edit rooms for selected events";

                    RectTransform transform = mismatch.transform as RectTransform;
                    RectTransform template = __instance.background;

                    transform.offsetMin = template.offsetMin;
                    transform.offsetMax = template.offsetMax;
                }

                if (RoomUtil.TryGetSelectedEventsRoomsUsage(out RoomsUsage usage))
                {
                    toggleContainer.SetActive(true);
                    applyTo.SetActive(true);
                    mismatch.SetActive(false);
                }
                else
                {
                    toggleContainer.SetActive(false);
                    applyTo.SetActive(false);
                    mismatch.SetActive(true);
                }
            }

            private static void Postfix(RDRoomsSelectionPopup __instance)
            {
                RoomUtil.Usage[] usage = RoomUtil.GetSelectedEventsUsage();

                int index = 0;
                foreach (var toggle in __instance.toggles)
                {
                    toggle.isOn = usage[index] != RoomUtil.Usage.UsedByNone;

                    if (usage[index] == RoomUtil.Usage.UsedBySome)
                    {
                        toggle.graphic.color = toggle.graphic.color.WithAlpha(0.5f);
                    }
                    else
                    {
                        toggle.graphic.color = toggle.graphic.color.WithAlpha(1.0f);
                    }

                    index++;
                }
            }

            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchStloc(0))
                    .Emit(OpCodes.Ldloc_0)
                    .EmitDelegate((bool flag) =>
                    {
                        return flag
                        && scnEditor.instance.selectedControls.All(control =>
                        control.levelEvent.roomsUsage == RoomsUsage.OneRoomOrOnTop
                        || control.levelEvent.roomsUsage == RoomsUsage.ManyRoomsAndOnTop);
                    });

                cursor
                    .Emit(OpCodes.Stloc_0);
            }

            private static GameObject toggleContainer = null;
            private static GameObject applyTo = null;
            private static GameObject mismatch = null;
        }
    }
}
