using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
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

            float offset = 0f; // GetRoomTabExtraOffset();

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                LayoutElement orderElement = eventControl.orderIcons[i].EnsureComponent<LayoutElement>();
                orderElement.layoutPriority = 99;

                LayoutElement roomElement = eventControl.roomIcons[i].EnsureComponent<LayoutElement>();
                roomElement.layoutPriority = 99;

                int extraRowCount = GetVisualRowCount(i);

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

        public override void UpdateUI(LevelEventControl_Base eventControl)
        {
            if (eventControl.levelEvent.IsFullTimelineHeight())
            {
                return;
            }
        }

        public override bool CanAllSelectedEventsBeDragged(int offset)
        {
            throw new NotImplementedException();
        }

        public override int GetDraggedEventYPosition(LevelEvent_Base levelEvent, int oldY)
        {
            throw new NotImplementedException();
        }

        public override int GetCurrentTabMaxUsedY()
        {
            int maxUsedY = 0; // GetRoomTabExtraRowOffset();
            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                maxUsedY += GetVisualRowCount(i) + 1;
            }

            return 0;
        }

        public override void UpdateTabPanelOnly()
        {
            float roomTabOffset = 0f; // GetRoomTabExtraOffset();
            float extraOffset = 0f;

            TabSection_Rooms tab = scnEditor.instance.tabSection_rooms;
            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                LayoutElement element = tab.labels[i].EnsureComponent<LayoutElement>();
                element.layoutPriority = 99;

                int extraRowCount = GetVisualRowCount(i);

                if (extraRowCount > 0)
                {
                    float heightOffset = scnEditor.instance.cellHeight * extraRowCount;

                    // I don't know why text.preferredHeight is 48 when first loading in? So let's just hardcode the 8 for now
                    // TODO figure this out lol

                    // Text text = tab.labels[i];
                    // element.preferredHeight = text.preferredHeight + heightOffset;

                    element.preferredHeight = 8f + heightOffset;
                    extraOffset += heightOffset;

                    element.enabled = true;
                }
                else
                {
                    element.enabled = false;
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

        public override void OverrideUsedRowCount(ref int usedRowCount)
        {
            usedRowCount = Math.Max(usedRowCount, scnEditor.instance.timeline.maxUsedY);
        }

        public override bool TryGetPreCreationEventData(int y, out int realY, out int realRow, out int visualRow)
        {
            throw new NotImplementedException();
        }

        public override void SetupJustCreatedEvent(LevelEvent_Base levelEvent)
        {
            throw new NotImplementedException();
        }

        private bool TryFindRoomForRow(int row, out int room, out int subRow)
        {
            //row -= GetRoomTabExtraRowOffset();
            //if (row < 0)
            //{
            //    row = 0;
            //}

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                int rowCount = GetVisualRowCount(i) + 1;
                row -= rowCount;

                if (row < 0)
                {
                    room = i;
                    subRow = rowCount + row;
                    return true;
                }
            }

            room = 0;
            subRow = 0;
            return false;
        }

        private int GetVisualRowCount(int room)
        {
            if (room >= RDEditorConstants.RoomCount
               || room < 0)
            {
                return 0;
            }

            //return SubRowStorage.Holder.roomData[room].usedSubRowCount;
            return SubRowStorage.Instance.GetRoomUsedSubRowCount(room);
        }

        private RectTransform roomLayoutGroupCache = null;
    }
}
