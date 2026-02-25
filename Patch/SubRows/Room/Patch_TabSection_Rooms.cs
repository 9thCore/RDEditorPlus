using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.SubRow;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.SubRows.Room
{
    internal static class Patch_TabSection_Rooms
    {
        [HarmonyPatch(typeof(TabSection_Rooms), nameof(TabSection_Rooms.Setup))]
        private static class Setup
        {
            private static void Postfix(TabSection_Rooms __instance)
            {
                SubRowStorage.Instance.SetupWithScrollMaskIntermediary(__instance.listRect, "Rooms");
                __instance.listRect.offsetMin = Vector2.zero;
                __instance.listRect.offsetMax = Vector2.zero;

                if (PluginConfig.PreviewScaleSubRowsEnabled)
                {
                    int index = 0;
                    foreach (RawImage preview in __instance.previews)
                    {
                        Transform border = preview.transform.parent;
                        Transform text = border.parent;
                        Text label = __instance.labels[index];

                        label.alignment = TextAnchor.UpperCenter;

                        GameObject group = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_RoomGroup{index++}");
                        group.transform.SetParent(text.parent);
                        group.transform.localPosition = Vector3.zero;
                        group.transform.localScale = Vector3.one;
                        group.layer = RDLayer.UI;

                        Image image = group.AddComponent<Image>();
                        image.color = Color.white.WithAlpha(0f);

                        LayoutElement element = group.EnsureComponent<LayoutElement>();
                        element.layoutPriority = 99;

                        text.SetParent(group.transform);
                        border.SetParent(group.transform);

                        RectTransform textRect = label.rectTransform;
                        textRect.anchorMin = Vector2.zero;
                        textRect.anchorMax = Vector2.one;
                        textRect.offsetMin = Vector2.zero;
                        textRect.offsetMax = Vector2.zero;

                        RectTransform previewBorderRect = border.GetComponent<RectTransform>();
                        previewBorderRect.anchorMin = Vector2.zero;
                        previewBorderRect.anchorMax = new Vector2(1.0f, 1.0f);
                        previewBorderRect.offsetMin = Vector2.zero;
                        previewBorderRect.offsetMax = Vector2.zero;

                        foreach (RectTransform child in previewBorderRect)
                        {
                            child.offsetMin = Vector2.zero;
                            child.offsetMax = Vector2.zero;
                        }

                        RDEventTrigger vanillaTrigger = preview.GetComponent<RDEventTrigger>();
                        RDEventTrigger trigger = group.EnsureComponent<RDEventTrigger>();

                        trigger.onPointerEnter = vanillaTrigger.onPointerEnter;
                        trigger.onPointerExit = vanillaTrigger.onPointerExit;

                        Object.Destroy(vanillaTrigger);

                        RoomManager.Instance.previewScalingElements.Add(element);
                        RoomManager.Instance.previewScalingRects.Add(group.GetComponent<RectTransform>());
                        RoomManager.Instance.previewBorders.Add(previewBorderRect);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TabSection_Rooms), nameof(TabSection_Rooms.Update))]
        private static class Update
        {
            private static void Postfix(TabSection_Rooms __instance)
            {
                RoomManager.Instance.UpdateTabScroll();
            }
        }
    }
}
