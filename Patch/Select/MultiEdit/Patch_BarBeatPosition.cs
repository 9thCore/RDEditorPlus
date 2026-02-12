using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_BarBeatPosition
    {
        [HarmonyPatch(typeof(BarBeatPosition), nameof(BarBeatPosition.Setup))]
        private static class Setup
        {
            private static void Postfix(BarBeatPosition __instance)
            {
                __instance.roomsContainer.transform.Find("overlay").GetComponent<Image>().sprite = RoomUtil.OverlaySprite;
            }
        }

        [HarmonyPatch(typeof(BarBeatPosition), nameof(BarBeatPosition.UpdateRoomButtons))]
        private static class UpdateRoomButtons
        {
            private static void ILManipulator(ILContext il, ILLabel retLabel)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchStloc(0))
                    .Emit(OpCodes.Ldarg_0)
                    .EmitDelegate((Button[] buttons) =>
                    {
                        const int OnTopIndex = 4;

                        RoomUtil.Usage[] usage = RoomUtil.GetSelectedEventsUsage();

                        for (int i = 0; i < RDEditorConstants.RoomCount; i++)
                        {
                            if (usage[i].HasFlag(RoomUtil.Usage.UsedBySome))
                            {
                                buttons[i].GetComponent<Image>().color = RoomUtil.GetRoomUsageColor(usage[i]);
                            }
                        }

                        bool onTopUsedBySome = usage[OnTopIndex].HasFlag(RoomUtil.Usage.UsedBySome);
                        buttons[OnTopIndex].gameObject.SetActive(onTopUsedBySome);
                        if (onTopUsedBySome)
                        {
                            buttons[OnTopIndex].GetComponent<Image>().color = RoomUtil.GetRoomUsageColor(usage[4]);
                        }

                        if (!RoomUtil.DoSelectedEventsHaveTheSameRoomsUsage())
                        {
                            scnEditor.instance.inspectorPanelManager.GetCurrent().position
                            .roomsContainer.transform.Find("overlay").GetComponent<Image>()
                            .color = Color.white;
                        }
                        else
                        {
                            scnEditor.instance.inspectorPanelManager.GetCurrent().position
                            .roomsContainer.transform.Find("overlay").GetComponent<Image>()
                            .color = Color.clear;
                        }
                    });

                cursor
                    .Emit(OpCodes.Br, retLabel);
            }
        }
    }
}
