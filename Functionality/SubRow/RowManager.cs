
using nn.fs;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.SubRow
{
    public class RowManager : BaseManager
    {
        private static RowManager instance;
        public static RowManager Instance
        {
            get
            {
                instance ??= new RowManager();
                return instance;
            }
        }

        public override bool CanAllSelectedEventsBeDragged(int offset)
        {
            foreach (LevelEventControl_Base control in scnEditor.instance.selectedControls)
            {
                int row = SubRowStorage.Instance.GetEventCorrectedRow(control.levelEvent);
                int newRow = row + offset;

                if (!TryFindPatientForRow(newRow, out _, out _))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetCurrentTabMaxUsedY()
        {
            int maxUsedY = 0;

            int room = scnEditor.instance.selectedRowsTabPageIndex;
            List<LevelEvent_MakeRow> rows = scnEditor.instance.rowsData;
            for (int i = 0; i < rows.Count; i++)
            {
                LevelEvent_MakeRow row = rows[i];

                if (row.room != room)
                {
                    continue;
                }

                maxUsedY += SubRowStorage.Instance.GetPatientVisualRowCount(row.uid);
            }

            return maxUsedY;
        }

        public override int GetDraggedEventYPosition(LevelEventControl_Base eventControl, int oldY)
        {
            if (!TryFindPatientForRow(oldY, out int patient, out int roomPosition))
            {
                return -(roomPosition - 1);
            }

            SubRowStorage.Instance.SetLevelEventRow(eventControl, patient);
            return roomPosition;
        }

        public override int? GetTimelineDisabledRowsValueThing()
        {
            return scnEditor.instance.timeline.maxUsedY;
        }

        public override void SetupJustCreatedEvent(LevelEvent_Base levelEvent)
        {
            // Nothing special
        }

        public override bool TryGetPreCreationEventData(int y, out int realY, out int realRow, out int visualRow)
        {
            if (!TryFindPatientForRow(y, out int patient, out int roomPosition))
            {
                realY = 0;
                realRow = 0;
                visualRow = 0;
                return false;
            }

            realRow = patient;
            realY = roomPosition;
            visualRow = y;
            return true;
        }

        public override void UpdateFullTimelineHeightEvent(LevelEventControl_Base eventControl)
        {
            // There should be no full timeline height events in the patients tab
            throw new InvalidOperationException();
        }

        public override void UpdateTab(bool force)
        {
            UpdatePage(force, scnEditor.instance.selectedRowsTabPageIndex);
            UpdateTabPanelOnly();
        }

        public override void UpdateTabPanelOnly()
        {
            float extraOffset = 0f;

            TabSection_Rows tab = scnEditor.instance.tabSection_rows;
            int room = scnEditor.instance.selectedRowsTabPageIndex;
            List<LevelEvent_MakeRow> rows = scnEditor.instance.rowsData;
            int index = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                LevelEvent_MakeRow row = rows[i];

                if (row.room != room)
                {
                    continue;
                }

                extraOffset += ResizeHeader(tab.rowHeaders[index],
                    SubRowStorage.Instance.GetPatientExtraVisualRowCount(row.uid));

                index++;
            }

            stillHasSpaceInThisRoom = false;

            for (; index < tab.rowHeaders.Count; index++)
            {
                stillHasSpaceInThisRoom = true;
                ResizeHeader(tab.rowHeaders[index], 0);
            }

            float scroll = float.IsNaN(scrollPosition) ? 0 : scrollPosition;

            tab.rowsListRect.OffsetMinY(scnEditor.instance.timeline.height
                - scnEditor.instance.cellHeight * RDEditorConstants.MaxRowsPerPage
                - extraOffset
                + scroll);

            tab.rowsListRect.OffsetMaxY(scroll);
        }

        public override void OverrideUsedRowCount(ref int usedRowCount)
        {
            usedRowCount = Math.Max(usedRowCount,
                scnEditor.instance.timeline.maxUsedY + (stillHasSpaceInThisRoom ? 1 : 0));
        }

        private bool TryFindPatientForRow(int row, out int patient, out int roomPosition)
        {
            return TryFindPatientForRow(scnEditor.instance.selectedRowsTabPageIndex, row, out patient, out roomPosition);
        }

        private bool TryFindPatientForRow(int room, int row, out int patient, out int roomPosition)
        {
            roomPosition = 0;

            if (row < 0)
            {
                patient = 0;
                return false;
            }

            foreach (LevelEvent_MakeRow makeRow in scnEditor.instance.rowsData)
            {
                if (makeRow.room != room)
                {
                    continue;
                }

                int rowCount = SubRowStorage.Instance.GetPatientVisualRowCount(makeRow.uid);
                row -= rowCount;

                if (row < 0)
                {
                    patient = makeRow.row;
                    return true;
                }

                roomPosition++;
            }

            patient = 0;
            return false;
        }

        private void UpdatePage(bool force, int page)
        {
            // If an event is above the first patient that got its size updated,
            // there's no point updating it as it definitely did not move.
            // ...Except if we're to force doing it anyway, as part of an undo
            bool updateEventPositions = force;

            List<LevelEvent_MakeRow> currentRows = scnEditor.instance.rowsData;
            foreach (LevelEvent_MakeRow row in currentRows)
            {
                if (row.room != page)
                {
                    continue;
                }

                int usedSubRowCount = 0;

                List<LevelEventControl_Base> controls = scnEditor.instance.eventControls_rows[row.row];
                foreach (LevelEventControl_Base control in controls)
                {
                    int num = SubRowStorage.Instance.GetSubRow(control.levelEvent) + 1;
                    usedSubRowCount = Math.Max(usedSubRowCount, num);

                    if (updateEventPositions)
                    {
                        control.UpdateUI();
                    }
                }

                updateEventPositions |= SubRowStorage.Instance.UpdatePatientUsedSubRowCountIfRequired(row.uid, usedSubRowCount);
            }
        }

        private float ResizeHeader(GameObject header, int extraRowCount)
        {
            LayoutElement element = header.transform.parent.EnsureComponent<LayoutElement>();
            element.layoutPriority = SubRowStorage.LayoutElementPriority;

            if (extraRowCount > 0)
            {
                float heightOffset = scnEditor.instance.cellHeight * extraRowCount;

                element.preferredHeight = heightOffset;
                element.enabled = true;

                return heightOffset;
            }

            element.enabled = false;
            return 0f;
        }

        private bool stillHasSpaceInThisRoom = false;
    }
}
