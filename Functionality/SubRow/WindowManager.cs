using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.Windows;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.SubRow
{
    public class WindowManager : BaseManager
    {
        private static WindowManager instance;
        public static WindowManager Instance
        {
            get
            {
                instance ??= new WindowManager();
                return instance;
            }
        }

        public static void RemoveInstance()
        {
            instance = null;
        }

        public override bool CanAllSelectedEventsBeDragged(int offset)
        {
            foreach (LevelEventControl_Base control in scnEditor.instance.selectedControls)
            {
                int row = SubRowStorage.Instance.GetEventCorrectedRow(control.levelEvent);
                int newRow = row + offset;

                if (!TryFindWindowForRow(newRow, out _))
                {
                    return false;
                }
            }

            return true;
        }

        public override int? GetCurrentTabMaxUsedY()
        {
            int maxUsedY = GetTimelineRowOffset();

            for (int i = 0; i < WindowCount; i++)
            {
                maxUsedY += SubRowStorage.Instance.GetWindowVisualRowCount(i);
            }

            return maxUsedY;
        }

        public override int GetDraggedEventYPosition(LevelEventControl_Base eventControl, int oldY)
        {
            if (!TryFindWindowForRow(oldY, out int window))
            {
                return -(WindowCount - 1);
            }

            if (!eventControl.levelEvent.IsPreCreationEvent())
            {
                SubRowStorage.Instance.SetLevelEventRow(eventControl, window);
            }

            eventControl.levelEvent.y = window;
            return window;
        }

        public override void SetupJustCreatedEvent(LevelEvent_Base levelEvent)
        {
            // Nothing special
        }

        public override bool TryGetPreCreationEventData(int y, out int realY, out int realRow, out int visualRow)
        {
            if (!TryFindWindowForRow(y, out int window))
            {
                realY = 0;
                realRow = 0;
                visualRow = 0;
                return false;
            }

            realY = window;
            realRow = window;
            visualRow = y;
            return true;
        }

        // this sucks
        public override void UpdateFullTimelineHeightEvent(LevelEventControl_Base eventControlBase)
        {
            if (eventControlBase is not LevelEventControl_Window eventControl)
            {
                Plugin.LogWarn($"{nameof(WindowManager)}.{nameof(UpdateFullTimelineHeightEvent)} called, but not for a {nameof(LevelEventControl_Window)}!!");
                return;
            }

            if (PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepFourRowsHigh)
            {
                return;
            }
            else if (PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepInSpecialRow)
            {
                eventControl.GetComponent<RectTransform>().OffsetMinY(-scnEditor.instance.cellHeight);

                int orderIndex = eventControl.levelEvent.type == LevelEventType.DesktopColor ? 1 : 0;

                for (int i = 0; i < eventControl.orderIcons.Length; i++)
                {
                    if (i == orderIndex)
                    {
                        continue;
                    }

                    eventControl.orderIcons[i].gameObject.SetActive(false);
                }

                eventControl.orderIcons[0].sprite = eventControl.orderSprites[0];

                RectTransform order = eventControl.orderIcons[orderIndex].GetComponent<RectTransform>();
                order.anchorMin = new Vector2(order.anchorMin.x, order.anchorMin.x);
                order.anchorMax = new Vector2(order.anchorMax.x, order.anchorMax.x);

                return;
            }

            int horizontalPadding = (int)(scnEditor.instance.timeline.zoomFactor * 2.0f);
            int verticalPadding = (int)(scnEditor.instance.timeline.zoomVertFactor * 2.0f);

            VerticalLayoutGroup orderGroup = eventControl.orderIcons[0].transform.parent.EnsureComponent<VerticalLayoutGroup>();
            orderGroup.childScaleHeight = true;
            orderGroup.padding = new RectOffset(horizontalPadding, horizontalPadding, verticalPadding, verticalPadding);

            float offset = GetTimelineCellOffset();

            if (PluginConfig.WindowsEnabled && PluginConfig.WindowsMoreEnabled)
            {
                float cellHeight = scnEditor.instance.cellHeight;
                var storage = eventControl.GetComponent<ReorderWindowStorage>();

                for (int i = 0; i < WindowCount; i++)
                {
                    var text = storage.TextForWindow(i);

                    LayoutElement orderElement = text.EnsureComponent<LayoutElement>();
                    orderElement.layoutPriority = 99;

                    int extraRowCount = SubRowStorage.Instance.GetWindowExtraVisualRowCount(i);

                    if (extraRowCount > 0)
                    {
                        float heightOffset = cellHeight * extraRowCount;

                        orderElement.preferredHeight = text.preferredHeight * (extraRowCount + 1);
                        offset += heightOffset;

                        orderElement.enabled = true;
                    }
                    else
                    {
                        orderElement.enabled = false;
                    }
                }

                eventControl.GetComponent<RectTransform>().OffsetMinY(-WindowCount * cellHeight);
            }
            else
            {
                orderGroup.reverseArrangement = true;

                for (int i = 0; i < RDEditorConstants.WindowCount; i++)
                {
                    LayoutElement orderElement = eventControl.orderIcons[i].EnsureComponent<LayoutElement>();
                    orderElement.layoutPriority = 99;

                    int extraRowCount = SubRowStorage.Instance.GetWindowExtraVisualRowCount(i);

                    if (extraRowCount > 0)
                    {
                        float heightOffset = scnEditor.instance.cellHeight * extraRowCount;

                        orderElement.preferredHeight = eventControl.orderIcons[i].preferredHeight * (extraRowCount + 1);
                        offset += heightOffset;

                        orderElement.enabled = true;
                    }
                    else
                    {
                        orderElement.enabled = false;
                    }
                }
            }

            eventControl.GetComponent<RectTransform>().OffsetMinRelativeY(-offset);
        }

        public override void UpdateTab(bool force)
        {
            int[] usedSubRowCounts = new int[WindowCount];

            for (int i = 0; i < WindowCount; i++)
            {
                usedSubRowCounts[i] = 0;
            }

            foreach (LevelEventControl_Base control in scnEditor.instance.eventControls_windows)
            {
                LevelEvent_Base levelEvent = control.levelEvent;

                if (levelEvent.IsFullTimelineHeight()
                    || levelEvent.IsPreCreationEvent())
                {
                    continue;
                }

                int num = SubRowStorage.Instance.GetSubRow(levelEvent) + 1;
                int window = levelEvent.y;

                if (window < usedSubRowCounts.Length)
                {
                    usedSubRowCounts[window] = Math.Max(usedSubRowCounts[window], num);
                }
            }

            int firstWindowThatUpdated = -1;

            if (!force)
            {
                firstWindowThatUpdated = WindowCount;

                for (int i = WindowCount - 1; i >= 0; i--)
                {
                    if (SubRowStorage.Instance.UpdateWindowUsedSubRowCountIfRequired(i, usedSubRowCounts[i]))
                    {
                        firstWindowThatUpdated = i;
                    }
                }
            }

            List<int> alternatingRows = new();
            for (int i = 0; i < WindowCount; i++)
            {
                alternatingRows.Add(usedSubRowCounts[i] + 1);
            }
            GeneralManager.Instance.SetAlternatingTimelineStrips(alternatingRows);

            foreach (LevelEventControl_Base control in scnEditor.instance.eventControls_windows)
            {
                int window = control.levelEvent.y;
                if (control.levelEvent.IsFullTimelineHeight()
                    || window > firstWindowThatUpdated)
                {
                    control.UpdateUI();
                }
            }

            UpdateTabPanelOnly();
        }

        public override void UpdateTabPanelOnly()
        {
            float windowTabOffset = GetTimelineCellOffset();
            float extraOffset = 0f;

            TabSection_Windows tab = scnEditor.instance.tabSection_windows;
            for (int i = 0; i < WindowCount; i++)
            {
                LayoutElement element = tab.labels[i].EnsureComponent<LayoutElement>();
                element.layoutPriority = 99;

                int extraRowCount = SubRowStorage.Instance.GetWindowExtraVisualRowCount(i);

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

            if (layoutGroupCache == null)
            {
                layoutGroupCache = tab.listRect
                    .GetComponentInChildren<VerticalLayoutGroup>()
                    .GetComponent<RectTransform>();
            }

            float scroll = float.IsNaN(scrollPosition) ? 0 : scrollPosition;

            layoutGroupCache.OffsetMinY(-windowTabOffset - extraOffset + scroll);
            layoutGroupCache.OffsetMaxY(-windowTabOffset + scroll);
        }

        public override int? GetTimelineDisabledRowsValueThing()
        {
            return scnEditor.instance.timeline.maxUsedY;
        }

        public override void OverrideUsedRowCount(ref int usedRowCount)
        {
            usedRowCount = Math.Max(usedRowCount, scnEditor.instance.timeline.maxUsedY);
        }

        public override bool AffectedByTallEventConfig()
        {
            return true;
        }

        public int GetTimelineRowOffset()
        {
            return PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepInSpecialRow ? 1 : 0;
        }

        public float GetTimelineCellOffset()
        {
            return GetTimelineRowOffset() * scnEditor.instance.cellHeight;
        }

        public override void UpdateTabScroll()
        {
            float y = scnEditor.instance.timeline.scrollViewVertContent.anchoredPosition.y;
            if (y == scrollPosition)
            {
                return;
            }

            scrollPosition = y;
            UpdateTabPanelOnly();
        }

        private bool TryFindWindowForRow(int row, out int window)
        {
            row -= GetTimelineRowOffset();
            if (row < 0)
            {
                window = 0;
                return false;
            }

            for (int i = 0; i < WindowCount; i++)
            {
                int rowCount = SubRowStorage.Instance.GetWindowVisualRowCount(i);
                row -= rowCount;

                if (row < 0)
                {
                    window = i;
                    return true;
                }
            }

            window = 0;
            return false;
        }

        private int WindowCount => MoreWindowManager.Instance.WindowCount;

        private RectTransform layoutGroupCache = null;
    }
}
