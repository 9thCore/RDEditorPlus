using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Util;
using RDLevelEditor;
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

                GameObject template = __instance.evTagRunToggle.graphic.gameObject;

                GameObject clone = GameObject.Instantiate(template);
                clone.name = InspectorUtil.MixedTagRunButtonGraphic;
                clone.transform.SetParent(template.transform.parent);
                clone.transform.localScale = Vector3.one;
                clone.gameObject.SetActive(false);

                RectTransform transform = clone.transform as RectTransform;
                RectTransform anchor = template.transform as RectTransform;
                transform.offsetMin = anchor.offsetMin;
                transform.offsetMax = anchor.offsetMax;

                clone.GetComponent<Image>().color = Color.white.WithAlpha(InspectorUtil.MixedTagRunButtonAlpha);
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
