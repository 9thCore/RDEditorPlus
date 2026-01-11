using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.RemoteConfigSettingsHelper;

namespace RDEditorPlus.ExtraData
{
    public class SubRowStorage : PagedStorage<SubRowStorage.EventInfo, SubRowStorage.PageInfo>
    {
        private static SubRowStorage holder;
        public static SubRowStorage Holder
        {
            get
            {
                holder ??= new SubRowStorage();
                return holder;
            }
        }

        public override void DecodeEvent(LevelEvent_Base levelEvent, Dictionary<string, object> properties)
        {
            if (levelEvent.IsFullTimelineHeight()
                || (levelEvent.IsSpriteEvent() && !PluginConfig.SpriteSubRowsEnabled)
                || (levelEvent.IsRoomEvent() && !PluginConfig.RoomSubRowsEnabled)
                || storage.ContainsKey(levelEvent.uid))
            {
                return;
            }

            if (!properties.TryGetValue(SubRowKey, out object value)
                   || value is not int subRow)
            {
                return;
            }

            storage.Add(levelEvent.uid, new EventInfo(subRow));
        }

        public override bool TryConstructJSONData(LevelEvent_Base levelEvent, out string data)
        {
            if (levelEvent.IsFullTimelineHeight())
            {
                data = string.Empty;
                return false;
            }

            if (storage.TryGetValue(levelEvent.uid, out EventInfo info)
                && info.subRow != 0)
            {
                data = $", \"{SubRowKey}\": {info.subRow}";
                return true;
            }

            data = string.Empty;
            return false;
        }

        public override bool TryRetrieveEventData(LevelEvent_Base levelEvent, out EventInfo info)
        {
            if (storage.TryGetValue(levelEvent.uid, out info))
            {
                return true;
            }

            info = null;
            return false;
        }

        public override EventInfo GetOrCreateEventData(LevelEvent_Base levelEvent)
        {
            if (storage.TryGetValue(levelEvent.uid, out EventInfo info))
            {
                return info;
            }

            info = new EventInfo(0);
            storage.Add(levelEvent.uid, info);
            return info;
        }

        public override void Clear()
        {
            base.Clear();
            roomScrollViewVertContentLastPosition = float.NaN;
        }

        public void HandleNewLevelEventControl(LevelEventControl_Base eventControl)
        {
            LevelEvent_Base levelEvent = eventControl.levelEvent;

            if (levelEvent.IsFullTimelineHeight())
            {
                return;
            }

            int subRow = 0;

            if (TryRetrieveEventData(levelEvent, out EventInfo info))
            {
                subRow = info.subRow;
            }

            if (PluginConfig.SpriteSubRowsEnabled && levelEvent.IsSpriteEvent())
            {
                PageInfo pageInfo = GetOrCreatePageDataForSprite(levelEvent.target);
                pageInfo.usedSubRowCount = Math.Max(pageInfo.usedSubRowCount, subRow + 1);
            } else if (PluginConfig.RoomSubRowsEnabled && levelEvent.IsRoomEvent())
            {
                PageInfo pageInfo = roomData[levelEvent.GetYValueAsValidRoom()];
                pageInfo.usedSubRowCount = Math.Max(pageInfo.usedSubRowCount, subRow + 1);
            }
        }

        public void OffsetLevelEventPosition(LevelEventControl_Base control)
        {
            float delta = GetLevelEventOffset(control.levelEvent);

            control.GetComponent<RectTransform>().AnchorRelativeY(-delta);
        }

        public void UpdateFullTimelineHeightRoomEvent(LevelEventControl_ShowRooms control)
        {
            if (PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepFourRowsHigh)
            {
                return;
            }
            //else if (PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepInSpecialRow)
            //{
            //    control.GetComponent<RectTransform>().OffsetMinY(-scnEditor.instance.cellHeight);

            //    for (int i = 1; i < RDEditorConstants.RoomCount; i++)
            //    {
            //        control.orderIcons[i].gameObject.SetActive(false);
            //        control.roomIcons[i].gameObject.SetActive(false);
            //    }

            //    control.orderIcons[0].sprite = control.orderSprites[0];
            //    control.roomIcons[0].Show(RoomTransitionType.Show, control.timeline.zoomVertFactor);

            //    return;
            //}

            int horizontalPadding = (int)(scnEditor.instance.timeline.zoomFactor * 2.0f);
            int verticalPadding = (int)(scnEditor.instance.timeline.zoomVertFactor * 2.0f);

            VerticalLayoutGroup orderGroup = control.orderIcons[0].transform.parent.EnsureComponent<VerticalLayoutGroup>();
            orderGroup.reverseArrangement = true;
            orderGroup.childScaleHeight = true;
            orderGroup.padding = new RectOffset(horizontalPadding, horizontalPadding, verticalPadding, verticalPadding);

            float offset = 0f;

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                LayoutElement orderElement = control.orderIcons[i].EnsureComponent<LayoutElement>();
                orderElement.layoutPriority = 99;

                LayoutElement roomElement = control.roomIcons[i].EnsureComponent<LayoutElement>();
                roomElement.layoutPriority = 99;

                int extraRowCount = GetRoomExtraRowCount(i);

                if (extraRowCount > 0)
                {
                    float heightOffset = scnEditor.instance.cellHeight * extraRowCount;

                    orderElement.preferredHeight = control.orderIcons[i].preferredHeight * (extraRowCount + 1);
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
            
            control.GetComponent<RectTransform>().OffsetMinRelativeY(-offset);
        }

        public void UpdateHeaders()
        {
            switch (scnEditor.instance.currentTab)
            {
                case Tab.Sprites:
                    UpdateSpriteHeaders();
                    break;
                case Tab.Rooms:
                    UpdateRoomHeaders();
                    break;
                default:
                    break;
            }
        }

        public void CorrectMaxUsedY()
        {
            scnEditor editor = scnEditor.instance;

            int maxUsedY = 0;

            switch (editor.currentTab)
            {
                case Tab.Sprites:
                    if (!PluginConfig.SpriteSubRowsEnabled)
                    {
                        return;
                    }

                    List<LevelEvent_MakeSprite> currentSprites = editor.currentPageSpritesData;
                    foreach (LevelEvent_MakeSprite sprite in currentSprites)
                    {
                        maxUsedY += GetSpriteExtraRowCount(sprite.spriteId) + 1;
                    }
                    break;
                case Tab.Rooms:
                    if (!PluginConfig.RoomSubRowsEnabled)
                    {
                        return;
                    }

                    for (int i = 0; i < RDEditorConstants.RoomCount; i++)
                    {
                        maxUsedY += GetRoomExtraRowCount(i) + 1;
                    }
                    break;
                default:
                    break;
            }

            editor.timeline.maxUsedY = maxUsedY;
        }

        private void UpdateRoomHeaders()
        {
            if (!PluginConfig.RoomSubRowsEnabled)
            {
                return;
            }

            TabSection_Rooms tab = scnEditor.instance.tabSection_rooms;

            //if (PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepInSpecialRow)
            //{
            //    if (roomExtraOffsetLayoutElement == null)
            //    {
            //        GameObject offset = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_RoomExtraOffset");
            //        roomExtraOffsetLayoutElement = offset.EnsureComponent<LayoutElement>();
            //        roomExtraOffsetLayoutElement.transform.SetParent(tab.labels[0].transform.parent);
            //    }

            //    roomExtraOffsetLayoutElement.preferredHeight = scnEditor.instance.cellHeight;
            //}

            float extraOffset = 0f;

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                LayoutElement element = tab.labels[i].EnsureComponent<LayoutElement>();
                element.layoutPriority = 99;

                int extraRowCount = GetRoomExtraRowCount(i);

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

            float scroll = float.IsNaN(roomScrollViewVertContentLastPosition) ? 0 : roomScrollViewVertContentLastPosition;

            roomLayoutGroupCache.OffsetMinY(-extraOffset + scroll);
            roomLayoutGroupCache.OffsetMaxY(scroll);
        }

        public void UpdateSpriteHeaders()
        {
            if (!PluginConfig.SpriteSubRowsEnabled)
            {
                return;
            }

            float extraOffset = 0;

            TabSection_Sprites tab = scnEditor.instance.tabSection_sprites;
            List<LevelEvent_MakeSprite> currentSprites = scnEditor.instance.currentPageSpritesData;
            for (int i = 0; i < tab.spriteHeaders.Count - 1; i++)
            {
                LayoutElement element = tab.spriteHeaders[i].EnsureComponent<LayoutElement>();
                element.layoutPriority = 99;

                int extraRowCount = GetSpriteExtraRowCount(currentSprites[i].spriteId);

                if (extraRowCount > 0)
                {
                    Image image = tab.spriteHeaders[i].GetComponent<Image>();

                    float heightOffset = scnEditor.instance.cellHeight * extraRowCount;

                    element.preferredHeight = image.preferredHeight + heightOffset;
                    extraOffset += heightOffset;

                    element.enabled = true;
                } else
                {
                    element.enabled = false;
                }
            }

            tab.headersListRect.SizeDeltaY(tab.spriteHeaders.Count * scnEditor.instance.cellHeight + extraOffset);
        }

        public int ModifyPlacementY(int y)
        {
            preCreationEventSubRow = 0;

            switch (scnEditor.instance.currentTab)
            {
                case Tab.Sprites:
                    if (!PluginConfig.SpriteSubRowsEnabled
                        || !TryFindSpriteForRow(y, out _, out int spritePositionInRoom, out int spriteSubRow))
                    {
                        return y;
                    }

                    preCreationEventSubRow = spriteSubRow;
                    return spritePositionInRoom;
                case Tab.Rooms:
                    if (!PluginConfig.RoomSubRowsEnabled
                        || !TryFindRoomForRow(y, out int room, out int roomSubRow))
                    {
                        return y;
                    }

                    preCreationEventSubRow = roomSubRow;
                    return room;
                default:
                    return y;
            }
        }

        public void FixPreCreationEventData(LevelEventControl_Base control)
        {
            LevelEvent_Base levelEvent = control.levelEvent;

            switch (scnEditor.instance.currentTab)
            {
                case Tab.Sprites:
                    if (!PluginConfig.SpriteSubRowsEnabled
                        || !TryFindSpriteForRow(levelEvent.y, out _, out int spritePositionInRoom, out int spriteSubRow))
                    {
                        return;
                    }

                    if (levelEvent.row != spritePositionInRoom)
                    {
                        // i got no clue why its sometimes added multiple times dude
                        while (control.container.Remove(control)) ;
                        levelEvent.row = spritePositionInRoom;
                        control.container.Add(control);
                    }

                    preCreationEventSubRow = spriteSubRow;
                    levelEvent.y = spritePositionInRoom;
                    return;
                case Tab.Rooms:
                    // Dragging the room event feels weird, but it's weird in vanilla too, so it's mostly fine

                    if (!PluginConfig.RoomSubRowsEnabled
                        || !TryFindRoomForRow(levelEvent.y, out int room, out int roomSubRow))
                    {
                        return;
                    }

                    preCreationEventSubRow = roomSubRow;
                    levelEvent.y = room;
                    return;
            }
        }

        public void SetupEvent(LevelEvent_Base levelEvent)
        {
            if (preCreationEventSubRow != 0)
            {
                GetOrCreateEventData(levelEvent).subRow = preCreationEventSubRow;
                preCreationEventSubRow = 0;
                UpdateCurrentTabSubRowUI(true);
            }
        }

        private float GetLevelEventOffset(LevelEvent_Base levelEvent)
        {
            if (levelEvent.IsFullTimelineHeight())
            {
                return 0f;
            }

            if (levelEvent.IsPreCreationEvent())
            {
                return GetPreCreationEventOffset(levelEvent);
            }

            if (PluginConfig.SpriteSubRowsEnabled && levelEvent.IsSpriteEvent())
            {
                return GetSpriteEventOffset(levelEvent);
            } else if (PluginConfig.RoomSubRowsEnabled && levelEvent.IsRoomEvent())
            {
                return GetRoomEventOffset(levelEvent);
            }

            return 0f;
        }

        private float GetPreCreationEventOffset(LevelEvent_Base levelEvent)
        {
            switch (scnEditor.instance.currentTab)
            {
                case Tab.Sprites:
                    return GetFirstSpritesInTabOffset(levelEvent.y) + preCreationEventSubRow * scnEditor.instance.cellHeight;
                default:
                    return 0f;
            }
        }

        private float GetRoomEventOffset(LevelEvent_Base levelEvent)
        {
            float offset = 0f;

            if (TryRetrieveEventData(levelEvent, out EventInfo info))
            {
                offset = info.subRow * scnEditor.instance.cellHeight;
            }

            for (int i = 0; i < levelEvent.y; i++)
            {
                offset += GetRoomExtraRowCount(i) * scnEditor.instance.cellHeight;
            }

            return offset;
        }

        private float GetSpriteEventOffset(LevelEvent_Base levelEvent)
        {
            List<LevelEvent_MakeSprite> sprites = scnEditor.instance.spritesData;
            string target = levelEvent.target;

            int room = -1;
            foreach (LevelEvent_MakeSprite sprite in sprites)
            {
                if (sprite.spriteId == target)
                {
                    room = sprite.room;
                    break;
                }
            }

            if (room == -1)
            {
                return 0f;
            }

            float offset = 0f;

            if (TryRetrieveEventData(levelEvent, out EventInfo info))
            {
                offset = info.subRow * scnEditor.instance.cellHeight;
            }

            foreach (LevelEvent_MakeSprite sprite in sprites)
            {
                if (sprite.spriteId == target)
                {
                    break;
                } else if (sprite.room == room)
                {
                    offset += GetSpriteExtraRowCount(sprite.spriteId) * scnEditor.instance.cellHeight;
                }
            }

            return offset;
        }

        private float GetFirstSpritesInTabOffset(int limit)
        {
            int extraRows = 0;

            foreach (LevelEvent_MakeSprite sprite in scnEditor.instance.currentPageSpritesData)
            {
                if (limit <= 0)
                {
                    break;
                }

                extraRows += GetSpriteExtraRowCount(sprite.spriteId);

                limit--;
            }

            return extraRows * scnEditor.instance.cellHeight;
        }

        private int GetSpriteExtraRowCount(string target)
        {
            if (spriteData.TryGetValue(target, out PageInfo info))
            {
                return info.usedSubRowCount;
            }

            return 0;
        }

        private int GetRoomExtraRowCount(int room)
        {
            if (room >= RDEditorConstants.RoomCount
                || room < 0)
            {
                return 0;
            }

            return roomData[room].usedSubRowCount;
        }

        public bool TryFindSpriteForRow(int row, out string id, out int roomPosition, out int subRow)
        {
            roomPosition = 0;

            foreach (LevelEvent_MakeSprite sprite in scnEditor.instance.currentPageSpritesData)
            {
                int rowCount = GetSpriteExtraRowCount(sprite.spriteId) + 1;
                row -= rowCount;

                if (row < 0)
                {
                    id = sprite.spriteId;
                    subRow = rowCount + row;
                    return true;
                }

                roomPosition++;
            }

            id = null;
            subRow = 0;
            return false;
        }

        public bool TryFindRoomForRow(int row, out int room, out int subRow)
        {
            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                int rowCount = GetRoomExtraRowCount(i) + 1;
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

        public void UpdateCurrentTabSubRowUI(bool force = false)
        {
            switch (scnEditor.instance.currentTab)
            {
                case Tab.Sprites:
                    UpdateCurrentSpriteTabSubRowUI(force);
                    break;
                case Tab.Rooms:
                    UpdateCurrentRoomTabSubRowUI(force);
                    break;
                default:
                    break;
            }
        }

        public bool GetCanDragFlagOverride(bool originalFlag, int row)
        {
            switch (scnEditor.instance.currentTab)
            {
                case Tab.Sprites:
                    return PluginConfig.SpriteSubRowsEnabled ? TryFindSpriteForRow(row, out _, out _, out _) : originalFlag;
                case Tab.Rooms:
                    return PluginConfig.RoomSubRowsEnabled ? TryFindRoomForRow(row, out _, out _) : originalFlag;
                default:
                    return originalFlag;
            }
        }

        public void OverrideUsedRowCount(ref int usedRowCount)
        {
            switch (scnEditor.instance.currentTab)
            {
                case Tab.Rooms:
                    if (!PluginConfig.RoomSubRowsEnabled)
                    {
                        return;
                    }

                    usedRowCount = Math.Max(usedRowCount, scnEditor.instance.timeline.maxUsedY);
                    break;
                default:
                    break;
            }
        }

        public void UpdateRoomTabScroll(TabSection_Rooms tab)
        {
            if (!PluginConfig.RoomSubRowsEnabled)
            {
                return;
            }

            float y = tab.timeline.scrollViewVertContent.anchoredPosition.y;
            if (y == roomScrollViewVertContentLastPosition)
            {
                return;
            }

            roomScrollViewVertContentLastPosition = y;
            UpdateRoomHeaders();
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

        private void UpdateCurrentRoomTabSubRowUI(bool force)
        {
            if (!PluginConfig.RoomSubRowsEnabled)
            {
                return;
            }

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

                int num = 1;

                if (TryRetrieveEventData(levelEvent, out EventInfo info))
                {
                    num = info.subRow + 1;
                }

                int room = levelEvent.GetYValueAsValidRoom();
                usedSubRowCounts[room] = Math.Max(usedSubRowCounts[room], num);
            }

            int firstRoomThatUpdated = -1;

            if (!force)
            {
                firstRoomThatUpdated = RDEditorConstants.RoomCount;

                for (int i = RDEditorConstants.RoomCount - 1; i >= 0; i--)
                {
                    if (usedSubRowCounts[i] != roomData[i].usedSubRowCount)
                    {
                        roomData[i].usedSubRowCount = usedSubRowCounts[i];
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

            UpdateRoomHeaders();
        }

        private void UpdateCurrentSpriteTabSubRowUI(bool force)
        {
            if (!PluginConfig.SpriteSubRowsEnabled)
            {
                return;
            }

            // If an event is above the first decoration that got its size updated,
            // there's no point updating it as it definitely did not move.
            // ...Except if we're to force doing it anyway, as part of an undo
            bool updateEventPositions = force;

            foreach (LevelEvent_MakeSprite sprite in scnEditor.instance.currentPageSpritesData)
            {
                int usedSubRowCount = 0;

                int index = SpriteHeader.GetSpriteDataIndex(sprite.spriteId);

                List<LevelEventControl_Base> controls = scnEditor.instance.eventControls_sprites[index];
                foreach (LevelEventControl_Base control in controls)
                {
                    int num = 1;

                    if (TryRetrieveEventData(control.levelEvent, out EventInfo info))
                    {
                        num = info.subRow + 1;
                    }

                    usedSubRowCount = Math.Max(usedSubRowCount, num);

                    if (updateEventPositions)
                    {
                        control.UpdateUI();
                    }
                }

                PageInfo pageInfo = GetOrCreatePageDataForSprite(sprite.spriteId);

                if (pageInfo.usedSubRowCount != usedSubRowCount)
                {
                    pageInfo.usedSubRowCount = usedSubRowCount;
                    updateEventPositions = true;
                }
            }

            UpdateSpriteHeaders();
        }

        public class EventInfo(int subRow)
        {
            public int subRow = subRow;
        }

        public class PageInfo()
        {
            public int usedSubRowCount = 0;
        }

        private LayoutElement roomExtraOffsetLayoutElement = null;
        private RectTransform roomLayoutGroupCache = null;
        private float roomScrollViewVertContentLastPosition = float.NaN;

        private int preCreationEventSubRow = 0;

        public const string SubRowKey = "mod_rdEditorPlus_subRow";
        public const string InternalHeaderSubRowKey = "_mod_rdEditorPlus_headerSubRow";
    }
}
