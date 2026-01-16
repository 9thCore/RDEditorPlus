using RDEditorPlus.Functionality.SubRow;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.ExtraData
{
    public class SubRowStorage
    {
        private static SubRowStorage instance;
        public static SubRowStorage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new();
                    instance.Clear();
                }
                return instance;
            }
        }

        public void DecodeEvent(LevelEvent_Base levelEvent, Dictionary<string, object> properties)
        {
            if (levelEvent.IsFullTimelineHeight()
                || (levelEvent.IsSpriteEvent() && !PluginConfig.SpriteSubRowsEnabled)
                || (levelEvent.IsRoomEvent() && !PluginConfig.RoomSubRowsEnabled))
            {
                return;
            }

            if (!properties.TryGetValue(SubRowKey, out object value)
                   || value is not int subRow
                   || subRow <= 0)
            {
                return;
            }

            eventData[levelEvent.uid] = new(subRow);
        }

        public bool TryConstructJSONData(LevelEvent_Base levelEvent, out string data)
        {
            if (levelEvent.IsFullTimelineHeight())
            {
                data = string.Empty;
                return false;
            }

            if (eventData.TryGetValue(levelEvent.uid, out EventData info)
                && info.subRow > 0)
            {
                data = $", \"{SubRowKey}\": {info.subRow}";
                return true;
            }

            data = string.Empty;
            return false;
        }

        public void SetSubRow(LevelEvent_Base levelEvent, int subRow)
        {
            if (eventData.TryGetValue(levelEvent.uid, out EventData data))
            {
                data.subRow = subRow;
                return;
            }

            eventData[levelEvent.uid] = new(subRow);
        }

        public void SetVisualRow(LevelEvent_Base levelEvent, int visualRow)
        {
            SetSubRow(levelEvent, visualRow - GetRowsAbove(levelEvent));
        }

        public int GetSubRow(LevelEvent_Base levelEvent)
        {
            if (eventData.TryGetValue(levelEvent.uid, out EventData info))
            {
                return info.subRow;
            }

            return 0;
        }

        public void Clear()
        {
            eventData.Clear();
            spriteData.Clear();
            windowData.Clear();
            rowData.Clear();
            GeneralManager.Instance.Clear();

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                roomData[i] = new(0);
            }
        }

        public int GetRoomExtraVisualRowCount(int room)
        {
            return roomData[room].usedSubRowCount;
        }

        public int GetRoomVisualRowCount(int room)
        {
            return GetRoomExtraVisualRowCount(room) + 1;
        }

        public bool UpdateRoomUsedSubRowCountIfRequired(int room, int usedSubRowCount)
        {
            if (roomData[room].usedSubRowCount != usedSubRowCount)
            {
                roomData[room].usedSubRowCount = usedSubRowCount;
                return true;
            }

            return false;
        }

        public int GetSpriteExtraVisualRowCount(string target)
        {
            if (spriteData.TryGetValue(target, out HeaderData data))
            {
                return data.usedSubRowCount;
            }

            return 0;
        }

        public int GetSpriteVisualRowCount(string target)
        {
            return 1 + GetSpriteExtraVisualRowCount(target);
        }

        public bool UpdateSpriteUsedSubRowCountIfRequired(string target, int usedSubRowCount)
        {
            if (!spriteData.TryGetValue(target, out HeaderData data))
            {
                spriteData[target] = new(usedSubRowCount);
                return true;
            }

            if (spriteData[target].usedSubRowCount != usedSubRowCount)
            {
                spriteData[target].usedSubRowCount = usedSubRowCount;
                return true;
            }

            return false;
        }

        public int GetWindowExtraVisualRowCount(int window)
        {
            if (window < windowData.Count)
            {
                return windowData[window].usedSubRowCount;
            }

            return 0;
        }

        public int GetWindowVisualRowCount(int window)
        {
            return GetWindowExtraVisualRowCount(window) + 1;
        }

        public bool UpdateWindowUsedSubRowCountIfRequired(int window, int usedSubRowCount)
        {
            if (window >= windowData.Count)
            {
                int start = windowData.Count;
                for (int i = start; i < window; i++)
                {
                    windowData.Add(new(0));
                }

                windowData.Add(new(usedSubRowCount));
                return true;
            }
        
            if (windowData[window].usedSubRowCount != usedSubRowCount)
            {
                windowData[window].usedSubRowCount = usedSubRowCount;
                return true;
            }

            return false;
        }

        public int GetPatientExtraVisualRowCount(int uid)
        {
            if (rowData.TryGetValue(uid, out HeaderData data))
            {
                return data.usedSubRowCount;
            }

            return 0;
        }

        public int GetPatientVisualRowCount(int uid)
        {
            return GetPatientExtraVisualRowCount(uid) + 1;
        }

        public bool UpdatePatientUsedSubRowCountIfRequired(int uid, int usedSubRowCount)
        {
            if (!rowData.TryGetValue(uid, out HeaderData data))
            {
                rowData[uid] = new(usedSubRowCount);
                return true;
            }

            if (data.usedSubRowCount != usedSubRowCount)
            {
                data.usedSubRowCount = usedSubRowCount;
                return true;
            }

            return false;
        }

        public int GetEventCorrectedRow(LevelEvent_Base levelEvent)
        {
            if (levelEvent.IsFullTimelineHeight())
            {
                return 0;
            }
            else if (levelEvent.IsPreCreationEvent())
            {
                return GetRowsAbove(levelEvent) + GeneralManager.Instance.PreCreationEventVisualRow;
            }
            else if (eventData.TryGetValue(levelEvent.uid, out EventData data))
            {
                return GetRowsAbove(levelEvent) + data.subRow;
            }

            return GetRowsAbove(levelEvent);
        }

        public void SetLevelEventTarget(LevelEventControl_Base eventControl, string target)
        {
            eventControl.levelEvent.target = target;
            eventControl.SetRow(SpriteHeader.GetSpriteDataIndex(target));
        }

        public void SetLevelEventRow(LevelEventControl_Base eventControl, int row)
        {
            eventControl.SetRow(row);
        }

        public void CopyData(LevelEvent_Base source, LevelEvent_Base destination)
        {
            if (eventData.TryGetValue(source.uid, out EventData data))
            {
                eventData[destination.uid] = data.Copy();
            }
        }

        public void SetupWithScrollMaskIntermediary(RectTransform rectTransform, string nameSuffix)
        {
            GameObject mask = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameSuffix}");

            mask.EnsureComponent<Image>();
            mask.EnsureComponent<Mask>().showMaskGraphic = false;
            mask.layer = rectTransform.gameObject.layer;

            int siblingIndex = rectTransform.GetSiblingIndex();

            RectTransform maskTransform = mask.GetComponent<RectTransform>();
            maskTransform.SetParent(rectTransform.parent);
            rectTransform.SetParent(maskTransform);

            maskTransform.SetSiblingIndex(siblingIndex);

            maskTransform.anchorMin = Vector2.zero;
            maskTransform.anchorMax = Vector2.one;
            maskTransform.offsetMin = new Vector2(2f, 0f);
            maskTransform.offsetMax = new Vector2(-1f, -16f);
        }

        private int GetRowsAbove(LevelEvent_Base levelEvent)
        {
            switch (levelEvent.GetTab())
            {
                case Tab.Sprites:
                    return GetNumberOfRowsAboveSprite(levelEvent.target);
                case Tab.Rooms:
                    return GetNumberOfRowsAboveRoom(levelEvent.y) + RoomManager.Instance.GetTimelineRowOffset();
                case Tab.Windows:
                    return GetNumberOfRowsAboveWindow(levelEvent.y) + WindowManager.Instance.GetTimelineRowOffset();
                case Tab.Rows:
                    return GetNumberOfRowsAbovePatient(levelEvent.row);
                default:
                    return 0;
            }
        }

        private int GetNumberOfRowsAboveSprite(string target)
        {
            LevelEvent_MakeSprite sprite = SpriteHeader.GetSpriteData(target);
            if (sprite == null)
            {
                return 0;
            }

            int room = sprite.room;
            int accumulated = 0;

            foreach (LevelEvent_MakeSprite sprite2 in scnEditor.instance.spritesData)
            {
                if (sprite2 == sprite)
                {
                    return accumulated;
                }

                if (sprite2.room != room)
                {
                    continue;
                }

                accumulated += GetSpriteVisualRowCount(sprite2.spriteId);
            }

            return 0;
        }

        private int GetNumberOfRowsAboveRoom(int room)
        {
            int accumulated = 0;

            for (int i = 0; i < room; i++)
            {
                accumulated += GetRoomVisualRowCount(i);
            }

            return accumulated;
        }

        private int GetNumberOfRowsAboveWindow(int window)
        {
            int accumulated = 0;

            for (int i = 0; i < window; i++)
            {
                accumulated += GetWindowVisualRowCount(i);
            }

            return accumulated;
        }

        private int GetNumberOfRowsAbovePatient(int row)
        {
            int accumulated = 0;
            int room = scnEditor.instance.rowsData[row].room;

            foreach (LevelEvent_MakeRow makeRow in scnEditor.instance.rowsData)
            {
                if (makeRow.row == row)
                {
                    break;
                }

                if (makeRow.room != room)
                {
                    continue;
                }

                accumulated += GetPatientVisualRowCount(makeRow.uid);
            }

            return accumulated;
        }

        private class EventData(int subRow)
        {
            public int subRow = subRow;

            public EventData Copy()
            {
                return new(subRow);
            }
        }

        private class HeaderData(int usedSubRowCount)
        {
            public int usedSubRowCount = usedSubRowCount;
        }

        private readonly Dictionary<int, EventData> eventData = new();
        private readonly Dictionary<string, HeaderData> spriteData = new();
        private readonly HeaderData[] roomData = new HeaderData[RDEditorConstants.RoomCount];
        private readonly List<HeaderData> windowData = new();
        private readonly Dictionary<int, HeaderData> rowData = new();

        public const string SubRowKey = "mod_rdEditorPlus_subRow";
        public const int LayoutElementPriority = 99;
    }
}
