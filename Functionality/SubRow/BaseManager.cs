using RDEditorPlus.ExtraData;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Functionality.SubRow
{
    public abstract class BaseManager
    {
        public abstract void UpdateFullTimelineHeightEvent(LevelEventControl_Base eventControl);

        public abstract bool CanAllSelectedEventsBeDragged(int offset);
        public abstract int GetDraggedEventYPosition(LevelEventControl_Base eventControl, int oldY);

        public abstract void UpdateTab(bool force);
        public abstract void UpdateTabPanelOnly();

        public abstract int? GetTimelineDisabledRowsValueThing();

        public abstract int? GetCurrentTabMaxUsedY();

        public abstract bool TryGetPreCreationEventData(int y, out int realY, out int realRow, out int visualRow);

        public abstract void SetupJustCreatedEvent(LevelEvent_Base levelEvent);

        public abstract bool AffectedByTallEventConfig();

        public virtual void UpdateUI(LevelEventControl_Base eventControl)
        {
            int num = SubRowStorage.Instance.GetEventCorrectedRow(eventControl.levelEvent);
            float position = scnEditor.instance.timeline.GetPosYFromRowIndex(num);
            eventControl.GetComponent<RectTransform>().AnchorPosY(position);
        }

        public virtual void UpdateTabScroll()
        {
            float y = scnEditor.instance.timeline.scrollViewVertContent.anchoredPosition.y;
            if (!Plugin.RDModificationsRowPatchEnabled
                && y == scrollPosition)
            {
                return;
            }

            scrollPosition = y;

            if (Plugin.RDModificationsRowPatchEnabled)
            {
                UpdateTab(force: false);
            } else
            {
                UpdateTabPanelOnly();
            }
        }

        public virtual void Clear()
        {
            scrollPosition = float.NaN;
        }

        public virtual void OverrideUsedRowCount(ref int usedRowCount)
        {
            // nothing
        }

        protected float scrollPosition = float.NaN;
    }
}
