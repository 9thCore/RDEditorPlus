using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.SubRow
{
    public class RoomManager : BaseManager
    {
        private static RoomManager instance;
        public static RoomManager Instance
        {
            get
            {
                instance ??= new RoomManager();
                return instance;
            }
        }

        public override void UpdateFullTimelineHeightEvent(LevelEventControl_Base eventControlBase)
        {
            if (eventControlBase is not LevelEventControl_ShowRooms eventControl)
            {
                Plugin.LogWarn($"{nameof(RoomManager)}.{nameof(UpdateFullTimelineHeightEvent)} called, but not for a {nameof(LevelEventControl_ShowRooms)}!!");
                return;
            }

            if (!eventControl.levelEvent.IsPreCreationEvent())
            {
                if (PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepFourRowsHigh)
                {
                    return;
                }
                else if (PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepInSpecialRow)
                {
                    eventControl.GetComponent<RectTransform>().OffsetMinY(-scnEditor.instance.cellHeight);

                    for (int i = 1; i < RDEditorConstants.RoomCount; i++)
                    {
                        eventControl.orderIcons[i].gameObject.SetActive(false);
                        eventControl.roomIcons[i].gameObject.SetActive(false);
                    }

                    eventControl.orderIcons[0].sprite = eventControl.orderSprites[0];

                    RectTransform order = eventControl.orderIcons[0].GetComponent<RectTransform>();
                    order.anchorMin = new Vector2(order.anchorMin.x, order.anchorMin.x);
                    order.anchorMax = new Vector2(order.anchorMax.x, order.anchorMax.x);

                    if (eventControl.roomIcons[0].gameObject.activeSelf)
                    {
                        eventControl.roomIcons[0].Show(RoomTransitionType.Show, eventControl.timeline.zoomVertFactor);
                    }

                    return;
                }
            }

            int horizontalPadding = (int)(scnEditor.instance.timeline.zoomFactor * 2.0f);
            int verticalPadding = (int)(scnEditor.instance.timeline.zoomVertFactor * 2.0f);

            VerticalLayoutGroup orderGroup = eventControl.orderIcons[0].transform.parent.EnsureComponent<VerticalLayoutGroup>();
            orderGroup.reverseArrangement = true;
            orderGroup.childScaleHeight = true;
            orderGroup.padding = new RectOffset(horizontalPadding, horizontalPadding, verticalPadding, verticalPadding);

            float offset = GetTimelineCellOffset();

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                LayoutElement orderElement = eventControl.orderIcons[i].EnsureComponent<LayoutElement>();
                orderElement.layoutPriority = 99;

                LayoutElement roomElement = eventControl.roomIcons[i].EnsureComponent<LayoutElement>();
                roomElement.layoutPriority = 99;

                int extraRowCount = SubRowStorage.Instance.GetRoomExtraVisualRowCount(i);

                if (extraRowCount > 0)
                {
                    float heightOffset = scnEditor.instance.cellHeight * extraRowCount;

                    orderElement.preferredHeight = eventControl.orderIcons[i].preferredHeight * (extraRowCount + 1);
                    roomElement.preferredHeight = heightOffset;
                    offset += heightOffset;

                    orderElement.enabled = true;
                    roomElement.enabled = true;
                }
                else
                {
                    orderElement.enabled = false;
                    roomElement.enabled = false;
                }
            }

            eventControl.GetComponent<RectTransform>().OffsetMinRelativeY(-offset);
        }

        public override bool CanAllSelectedEventsBeDragged(int offset)
        {
            foreach (LevelEventControl_Base control in scnEditor.instance.selectedControls)
            {
                int row = SubRowStorage.Instance.GetEventCorrectedRow(control.levelEvent);
                int newRow = row + offset;

                if (!TryFindRoomForRow(newRow, out _))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetDraggedEventYPosition(LevelEventControl_Base eventControl, int oldY)
        {
            if (!TryFindRoomForRow(oldY, out int room))
            {
                return -(RDEditorConstants.RoomCount - 1);
            }

            if (!eventControl.levelEvent.IsPreCreationEvent())
            {
                SubRowStorage.Instance.SetLevelEventRow(eventControl, room);
            }

            eventControl.levelEvent.y = room;
            return room;
        }

        public override int? GetCurrentTabMaxUsedY()
        {
            int maxUsedY = GetTimelineRowOffset();

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                maxUsedY += SubRowStorage.Instance.GetRoomVisualRowCount(i);
            }

            return maxUsedY;
        }

        public override void UpdateTabPanelOnly()
        {
            const float baseTextHeight = 8f;

            float roomTabOffset = GetTimelineCellOffset();
            float extraOffset = 0f;

            TabSection_Rooms tab = scnEditor.instance.tabSection_rooms;
            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                LayoutElement element = tab.labels[i].EnsureComponent<LayoutElement>();
                element.layoutPriority = 99;

                int extraRowCount = SubRowStorage.Instance.GetRoomExtraVisualRowCount(i);

                if (extraRowCount > 0)
                {
                    float heightOffset = scnEditor.instance.cellHeight * extraRowCount;

                    element.preferredHeight = baseTextHeight + heightOffset;
                    extraOffset += heightOffset;

                    element.enabled = true;
                }
                else
                {
                    element.enabled = false;
                }

                if (PluginConfig.PreviewScaleSubRowsEnabled)
                {
                    LayoutElement element2 = previewScalingElements[i];

                    int rows = SubRowStorage.Instance.GetRoomVisualRowCount(i);
                    float height = rows * scnEditor.instance.cellHeight;

                    element2.preferredHeight = height;

                    bool visible = rows >= PluginConfig.PreviewScaleSubRowsMinimum;
                    previewBorders[i].gameObject.SetActive(visible);

                    if (visible)
                    {
                        const float roomPreviewAspectRatio = 18f / 11f;
                        const float center = 0.5f;
                        const float borderWidth = 1f;

                        float previewHeight = height - baseTextHeight;
                        float previewWidth = previewHeight * roomPreviewAspectRatio;

                        float parentWidth = previewScalingRects[i].sizeDelta.x;
                        float parentHeight = height;

                        float anchorWidth = previewWidth / parentWidth;
                        float anchorHeight = previewHeight / parentHeight;

                        float verticalOffset = 0f;

                        if (anchorWidth > 1f)
                        {
                            float factor = 1f / anchorWidth;
                            float oldHeight = anchorHeight;

                            anchorWidth *= factor;
                            anchorHeight *= factor;

                            previewWidth *= factor;
                            previewHeight *= factor;

                            verticalOffset = (oldHeight - anchorHeight) / 2f;
                        }

                        float minX = -anchorWidth / 2f + center;
                        float maxX = anchorWidth / 2f + center;

                        previewBorders[i].anchorMin = new Vector2(minX, verticalOffset);
                        previewBorders[i].anchorMax = new Vector2(maxX, anchorHeight + verticalOffset);

                        float childAnchorWidth = (previewWidth - borderWidth * 2f) / previewWidth;
                        float childAnchorHeight = (previewHeight - borderWidth * 2f) / previewHeight;

                        Vector2 childAnchorMin = new Vector2(
                            -childAnchorWidth / 2f + center,
                            -childAnchorHeight / 2f + center);

                        Vector2 childAnchorMax = new Vector2(
                             childAnchorWidth / 2f + center, 
                             childAnchorHeight / 2f + center);

                        foreach (RectTransform child in previewBorders[i])
                        {
                            child.anchorMin = childAnchorMin;
                            child.anchorMax = childAnchorMax;
                        }
                    }
                }
            }

            if (roomLayoutGroupCache == null)
            {
                roomLayoutGroupCache = tab.listRect
                    .GetComponentInChildren<VerticalLayoutGroup>()
                    .GetComponent<RectTransform>();
            }

            float scroll = float.IsNaN(scrollPosition) ? 0 : scrollPosition;

            roomLayoutGroupCache.OffsetMinY(-roomTabOffset - extraOffset + scroll);
            roomLayoutGroupCache.OffsetMaxY(-roomTabOffset + scroll);
        }

        public override void UpdateTab(bool force)
        {
            int[] usedSubRowCounts = new int[RDEditorConstants.RoomCount];

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                usedSubRowCounts[i] = 0;
            }

            foreach (LevelEventControl_Base control in scnEditor.instance.eventControls_rooms)
            {
                LevelEvent_Base levelEvent = control.levelEvent;

                if (levelEvent.IsFullTimelineHeight()
                    || levelEvent.IsPreCreationEvent())
                {
                    continue;
                }

                int num = SubRowStorage.Instance.GetSubRow(levelEvent) + 1;
                int room = levelEvent.GetYValueAsValidRoom();
                usedSubRowCounts[room] = Math.Max(usedSubRowCounts[room], num);
            }

            List<int> alternatingRows = new();
            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                alternatingRows.Add(usedSubRowCounts[i] + 1);
            }
            GeneralManager.Instance.SetAlternatingTimelineStrips(alternatingRows);

            int firstRoomThatUpdated = -1;

            if (!force)
            {
                firstRoomThatUpdated = RDEditorConstants.RoomCount;

                for (int i = RDEditorConstants.RoomCount - 1; i >= 0; i--)
                {
                    if (SubRowStorage.Instance.UpdateRoomUsedSubRowCountIfRequired(i, usedSubRowCounts[i]))
                    {
                        firstRoomThatUpdated = i;
                    }
                }
            }

            foreach (LevelEventControl_Base control in scnEditor.instance.eventControls_rooms)
            {
                int room = control.levelEvent.GetYValueAsValidRoom();
                if (control.levelEvent.IsFullTimelineHeight()
                    || room > firstRoomThatUpdated)
                {
                    control.UpdateUI();
                }
            }

            UpdateTabPanelOnly();
        }

        public override bool TryGetPreCreationEventData(int y, out int realY, out int realRow, out int visualRow)
        {
            if (!TryFindRoomForRow(y, out int room))
            {
                realY = 0;
                realRow = 0;
                visualRow = 0;
                return false;
            }

            realY = room;
            realRow = room;
            visualRow = y;
            return true;
        }

        public override void SetupJustCreatedEvent(LevelEvent_Base levelEvent)
        {
            // Nothing special
        }

        public override int? GetTimelineDisabledRowsValueThing()
        {
            return scnEditor.instance.timeline.maxUsedY;
        }

        public override void OverrideUsedRowCount(ref int usedRowCount)
        {
            usedRowCount = Math.Max(usedRowCount, scnEditor.instance.timeline.maxUsedY);
        }

        public int GetTimelineRowOffset()
        {
            return PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepInSpecialRow ? 1 : 0;
        }

        public float GetTimelineCellOffset()
        {
            return GetTimelineRowOffset() * scnEditor.instance.cellHeight;
        }

        private bool TryFindRoomForRow(int row, out int room)
        {
            row -= GetTimelineRowOffset();
            if (row < 0)
            {
                room = 0;
                return false;
            }

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                int rowCount = SubRowStorage.Instance.GetRoomVisualRowCount(i);
                row -= rowCount;

                if (row < 0)
                {
                    room = i;
                    return true;
                }
            }

            room = 0;
            return false;
        }

        public readonly List<LayoutElement> previewScalingElements = new();
        public readonly List<RectTransform> previewScalingRects = new();
        public readonly List<RectTransform> previewBorders = new();

        private RectTransform roomLayoutGroupCache = null;
    }
}
