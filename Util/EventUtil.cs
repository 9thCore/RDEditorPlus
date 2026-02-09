using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace RDEditorPlus.Util
{
    public static class EventUtil
    {
        public static bool IsSpriteEvent(this LevelEvent_Base levelEvent)
        {
            return levelEvent.isSpriteTabEvent
                && levelEvent.target != null;
        }

        // Dummy
        public static bool IsSoundEvent(this LevelEvent_Base levelEvent)
        {
            return false;
        }

        public static bool IsRowEvent(this LevelEvent_Base levelEvent)
        {
            return levelEvent.isClassicRowEvent || levelEvent.isOneshotRowEvent;
        }

        public static bool IsRoomEvent(this LevelEvent_Base levelEvent)
        {
            return RDEditorConstants.levelEventTabs.TryGetValue(Tab.Rooms, out List<LevelEventType> list)
                && list.Contains(levelEvent.type);
        }

        // Dummy
        public static bool IsActionEvent(this LevelEvent_Base levelEvent)
        {
            return false;
        }

        // Dummy
        public static bool IsWindowEvent(this LevelEvent_Base levelEvent)
        {
            return false;
        }

        public static bool IsPreCreationEvent(this LevelEvent_Base levelEvent)
        {
            return levelEvent.type == LevelEventType.None
                && (scnEditor.instance.selectedControl == null
                || scnEditor.instance.selectedControl.levelEvent == levelEvent);
        }

        public static int GetYValueAsValidRoom(this LevelEvent_Base levelEvent)
        {
            if (levelEvent.y < 0)
            {
                return 0;
            }

            if (levelEvent.y > RDEditorConstants.RoomCount - 1)
            {
                return RDEditorConstants.RoomCount - 1;
            }

            return levelEvent.y;
        }

        public static bool IsFullTimelineHeight(this LevelEvent_Base levelEvent)
        {
            return levelEvent.type == LevelEventType.ReorderRooms
                || levelEvent.type == LevelEventType.ShowRooms
                || levelEvent.type == LevelEventType.ReorderWindows
                || levelEvent.type == LevelEventType.DesktopColor;
        }

        public static bool IsMultiTabEvent(this LevelEvent_Base levelEvent)
        {
            return levelEvent.type == LevelEventType.Comment;
        }

        public static Tab GetTab(this LevelEvent_Base levelEvent)
        {
            if (levelEvent.IsMultiTabEvent())
            {
                return levelEvent.tab;
            }

            return levelEvent.defaultTab;
        }

        public static void SetRow(this LevelEventControl_Base eventControl, int row)
        {
            if (eventControl.levelEvent.row == row)
            {
                return;
            }

            while (eventControl.container.Remove(eventControl)) ;
            eventControl.levelEvent.row = row;
            eventControl.container.Add(eventControl);
        }

        public static void UpdateVFXPresetDropdown()
        {
            InspectorPanel_SetVFXPreset panel = scnEditor.instance.inspectorPanelManager.Get<InspectorPanel_SetVFXPreset>();

            PropertyControl_Dropdown control = panel.properties
                .Select(property => property.control as PropertyControl_Dropdown)
                .Where(control => control != null)
                .First();

            InspectorPanel_SetVFXPreset.UpdatePresetsToDisplay();
            control.SetDropdownOptions(InspectorPanel_SetVFXPreset.displayedPresets, localize: true);

            foreach (RDThemeFX preset in InspectorPanel_SetVFXPreset.legacyPresetsInLevel)
            {
                int index = Array.IndexOf(InspectorPanel_SetVFXPreset.displayedPresets, preset);
                Dropdown.OptionData optionData = control.dropdown.options[index];
                optionData.text = "<color=#8b0000>" + optionData.text + "</color>";
            }
        }
    }
}
