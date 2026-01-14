using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using UnityEngine;

namespace RDEditorPlus.Functionality.SubRow
{
    public class GeneralManager : BaseManager
    {
        private static GeneralManager instance;
        public static GeneralManager Instance
        {
            get
            {
                instance ??= new GeneralManager();
                return instance;
            }
        }

        public override bool CanAllSelectedEventsBeDragged(int offset)
        {
            throw new InvalidOperationException();
        }

        public override int GetDraggedEventYPosition(LevelEventControl_Base eventControl, int oldY)
        {
            if (currentTabController == null)
            {
                return oldY;
            }

            int newY = currentTabController.GetDraggedEventYPosition(eventControl, oldY);

            if (newY < 0)
            {
                return oldY < 0 ? 0 : -newY;
            }

            if (eventControl.levelEvent.IsPreCreationEvent())
            {
                preCreationEventVisualRow = oldY;
            }
            else
            {
                SubRowStorage.Instance.SetVisualRow(eventControl.levelEvent, oldY);
            }

            return newY;
        }

        public override void UpdateFullTimelineHeightEvent(LevelEventControl_Base eventControl)
        {
            currentTabController?.UpdateFullTimelineHeightEvent(eventControl);
        }

        public override int GetCurrentTabMaxUsedY()
        {
            return currentTabController?.GetCurrentTabMaxUsedY() ?? 0;
        }

        public override void UpdateTab(bool force)
        {
            currentTabController?.UpdateTab(force);
        }

        public override void UpdateTabPanelOnly()
        {
            currentTabController?.UpdateTabPanelOnly();
        }

        public override void OverrideUsedRowCount(ref int usedRowCount)
        {
            currentTabController?.OverrideUsedRowCount(ref usedRowCount);
        }

        public override void Clear()
        {
            base.Clear();
            roomController.Clear();
            spriteController.Clear();

            preCreationEventVisualRow = 0;
        }

        public override bool TryGetPreCreationEventData(int y, out int realY, out int realRow, out int visualRow)
        {
            throw new InvalidOperationException();
        }

        public override void SetupJustCreatedEvent(LevelEvent_Base levelEvent)
        {
            if (preCreationEventVisualRow != 0)
            {
                levelEvent.tab = scnEditor.instance.currentTab; // because comments suck
                currentTabController?.SetupJustCreatedEvent(levelEvent);
                SubRowStorage.Instance.SetVisualRow(levelEvent, preCreationEventVisualRow);
                UpdateTab(force: true);
            }

            preCreationEventVisualRow = 0;
        }

        public bool CanAllSelectedEventsBeDragged(bool originalFlag, int offset)
        {
            return currentTabController?.CanAllSelectedEventsBeDragged(offset / scnEditor.instance.cellHeight) ?? originalFlag;
        }

        public void HandleNewLevelEventControl(LevelEventControl_Base eventControl)
        {
            LevelEvent_Base levelEvent = eventControl.levelEvent;

            if (levelEvent.IsFullTimelineHeight())
            {
                return;
            }
        }

        public int ModifyPointerClickYPosition(int y)
        {
            if (currentTabController == null
                || !currentTabController.TryGetPreCreationEventData(y, out int realY, out _, out int visualRow))
            {
                return y;
            }

            preCreationEventVisualRow = visualRow;
            return realY;
        }

        public void ChangeTab(Tab tab)
        {
            currentTabController = tab switch
            {
                Tab.Sprites => PluginConfig.SpriteSubRowsEnabled ? spriteController : null,
                Tab.Rooms => PluginConfig.RoomSubRowsEnabled ? roomController : null,
                _ => null,
            };

            if (tab != Tab.Sprites)
            {
                UpdateTab(force: false);
            }
        }

        //private void FixPreCreationEventControl(LevelEventControl_Base eventControl)
        //{
        //    LevelEvent_Base levelEvent = eventControl.levelEvent;

        //    if (currentTabController == null
        //        || !currentTabController.TryGetPreCreationEventData(levelEvent.y, out int realY, out int realRow, out int visualRow))
        //    {
        //        return;
        //    }

        //    if (levelEvent.row != realRow)
        //    {
        //        // i got no clue why its sometimes added multiple times
        //        while (eventControl.container.Remove(eventControl)) ;
        //        levelEvent.row = realRow;
        //        eventControl.container.Add(eventControl);
        //    }

        //    preCreationEventVisualRow = visualRow;
        //    levelEvent.y = realY;
        //}

        public int PreCreationEventVisualRow => preCreationEventVisualRow;

        private int preCreationEventVisualRow = 0;

        private BaseManager currentTabController = null;
        private readonly SpriteManager spriteController = SpriteManager.Instance;
        private readonly RoomManager roomController = RoomManager.Instance;
    }
}
