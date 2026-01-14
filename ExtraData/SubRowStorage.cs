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
                instance ??= new SubRowStorage();
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

        public void SetRoomUsedSubRowCount(int room, int usedSubRowCount)
        {
            roomData[room] = new(usedSubRowCount);
        }

        public int GetRoomUsedSubRowCount(int room)
        {
            return roomData[room].usedSubRowCount;
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

        public void Clear()
        {
            eventData.Clear();
            spriteData.Clear();
            GeneralManager.Instance.Clear();

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                roomData[i] = new(0);
            }
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

        public int GetNumberOfRowsAboveSprite(string target)
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

        public void SetLevelEventTarget(LevelEvent_Base levelEvent, string target)
        {
            levelEvent.target = target;
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
                default:
                    return 0;
            }
        }

        //private EventInfo GetEventInfo(LevelEvent_Base levelEvent)
        //{
        //    return levelEvent.GetTab() switch
        //    {
        //        Tab.Sprites => new SpriteEventInfo(levelEvent),
        //        _ => null
        //    };
        //}

        //public abstract class EventInfo
        //{
        //    public int visualRow, subRow;

        //    public abstract void SetRow(int row);
        //    public abstract void UpdateVisualRow();
        //    public abstract void UpdateSubRow();
        //    public abstract void UpdateSubRowData();
        //}

        //public class SpriteEventInfo(LevelEvent_Base levelEvent) : EventInfo
        //{
        //    public string spriteID = !string.IsNullOrEmpty(levelEvent.target)
        //        ? levelEvent.target
        //        : scnEditor.instance.spritesData[levelEvent.row].spriteId;

        //    public override void SetRow(int row)
        //    {
        //        spriteID = scnEditor.instance.spritesData[row].spriteId;
        //    }

        //    public override void UpdateVisualRow()
        //    {
        //        PageInfo info = Holder.GetOrCreatePageDataForSprite(spriteID);
        //        visualRow = info.rowsAbove + subRow;
        //    }

        //    public override void UpdateSubRow()
        //    {
        //        PageInfo info = Holder.GetOrCreatePageDataForSprite(spriteID);
        //        subRow = visualRow - info.rowsAbove;
        //    }

        //    public override void UpdateSubRowData()
        //    {
        //        PageInfo info = Holder.GetOrCreatePageDataForSprite(spriteID);
        //        info.usedSubRowCount = Math.Max(info.usedSubRowCount, subRow);
        //    }
        //}

        //public class PageInfo
        //{
        //    public int usedSubRowCount = 0;
        //    public int rowsAbove = 0;
        //}

        private class EventData(int subRow)
        {
            public int subRow = subRow;
        }

        private class HeaderData(int usedSubRowCount)
        {
            public int usedSubRowCount = usedSubRowCount;
        }

        private readonly Dictionary<int, EventData> eventData = new();
        private readonly Dictionary<string, HeaderData> spriteData = new();
        private readonly HeaderData[] roomData = new HeaderData[RDEditorConstants.RoomCount];

        public const string SubRowKey = "mod_rdEditorPlus_subRow";
        public const int LayoutElementPriority = 99;
    }
}
