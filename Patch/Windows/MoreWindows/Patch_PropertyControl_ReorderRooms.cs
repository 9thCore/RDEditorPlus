using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDLevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal static class Patch_PropertyControl_ReorderRooms
    {
        [HarmonyPatch(typeof(PropertyControl_ReorderRooms), nameof(PropertyControl_ReorderRooms.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_ReorderRooms __instance)
            {
                if (!__instance.controlAttribute.isWindow)
                {
                    return;
                }

                foreach (var image in __instance.GetComponentsInChildren<Image>())
                {
                    if (image.gameObject.name == "icons")
                    {
                        image.gameObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PropertyControl_ReorderRooms), nameof(PropertyControl_ReorderRooms.Update))]
        private static class Update
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStloc(2))
                    .Emit(OpCodes.Ldarg_0)
                    .EmitDelegate((float oldBottomBounds, PropertyControl_ReorderRooms propertyControl) =>
                    {
                        if (!propertyControl.controlAttribute.isWindow)
                        {
                            return oldBottomBounds;
                        }

                        return (MoreWindowManager.Instance.WindowCount - 1) * PropertyControl_ReorderRooms.offsetY;
                    });
            }
        }

        [HarmonyPatch(typeof(PropertyControl_ReorderRooms), nameof(PropertyControl_ReorderRooms.UpdateUI))]
        private static class UpdateUI
        {
            private static void Prefix(PropertyControl_ReorderRooms __instance)
            {
                if (!__instance.controlAttribute.isWindow)
                {
                    return;
                }

                int windowCount = MoreWindowManager.Instance.WindowCount;

                EnsureControls(__instance, windowCount);

                int num = 0;
                foreach (var room in __instance.roomButtons)
                {
                    room.gameObject.SetActive(num < windowCount);
                    num++;
                }
            }

            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor.ReplaceNextFourWithWindowCount();
            }

            private static void Postfix(PropertyControl_ReorderRooms __instance)
            {
                if (!__instance.controlAttribute.isWindow)
                {
                    return;
                }

                __instance.SetPropertyControlHeight(-__instance.ordered.Last().offsetMin.y - PropertyControl_ReorderRooms.offsetY, updateProperty: true);
            }
        }

        [HarmonyPatch(typeof(PropertyControl_ReorderRooms), nameof(PropertyControl_ReorderRooms.Save))]
        private static class Save
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor.ReplaceNextFourWithWindowCount();
                cursor.ReplaceNextFourWithWindowCount();
            }
        }

        private static void ReplaceNextFourWithWindowCount(this ILCursor cursor)
        {
            cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchLdcI4(4))
                    .Emit(OpCodes.Ldarg_0)
                    .EmitDelegate(FourReplacer);
        }

        private static int FourReplacer(int oldValue, PropertyControl_ReorderRooms propertyControl)
        {
            return propertyControl.controlAttribute.isWindow ? MoreWindowManager.Instance.WindowCount : oldValue;
        }

        private static void EnsureControls(PropertyControl_ReorderRooms propertyControl, int count)
        {
            if (propertyControl.roomButtons.Length >= count)
            {
                return;
            }

            List<RectTransform> newRoomButtons = new(propertyControl.roomButtons);

            GameObject template = propertyControl.roomButtons[0].gameObject;
            Transform parent = template.transform.parent;

            for (int i = propertyControl.roomButtons.Length; i < count; i++)
            {
                propertyControl.ordered.Add(default);

                GameObject clone = GameObject.Instantiate(template);
                clone.name = i.ToString();

                clone.transform.SetParent(parent);
                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition = Vector3.zero;

                var text = clone.GetComponentInChildren<Text>();
                text.text = $"{RDString.Get("editor.window")} {i + 1}";
                RDStringToUIText.Apply(text, changeFontForLanguage: true, isStatusText: false, scale: 0f);

                var rt = clone.transform as RectTransform;
                rt.offsetMax = Vector2.zero;

                clone.GetComponent<RDEventTrigger>().onPointerDown = _ =>
                {
                    propertyControl.buttonBeingDragged = rt;
                    propertyControl.initialDragOffset = Input.mousePosition.y - rt.position.y;
                    rt.SetAsLastSibling();
                };

                newRoomButtons.Add(rt);
            }

            propertyControl.roomButtons = newRoomButtons.ToArray();
        }
    }
}
