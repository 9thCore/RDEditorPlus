using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;

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
            if ((levelEvent.IsSpriteEvent() && !PluginConfig.SpriteSubRowsEnabled)
                || (false)
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

        public void HandleNewLevelEventControl(LevelEventControl_Base eventControl)
        {
            LevelEvent_Base levelEvent = eventControl.levelEvent;

            int subRow = 0;

            if (Holder.TryRetrieveEventData(levelEvent, out EventInfo info))
            {
                subRow = info.subRow;
            }

            if (PluginConfig.SpriteSubRowsEnabled && levelEvent.IsSpriteEvent())
            {
                PageInfo pageInfo = Holder.GetOrCreatePageDataForSprite(levelEvent.target);
                pageInfo.usedSubRowCount = Math.Max(pageInfo.usedSubRowCount, subRow + 1);
            }
        }

        public void OffsetLevelEvent(LevelEventControl_Base control)
        {
            RectTransform transform = control.GetComponent<RectTransform>();

            if (control.levelEvent.IsSpriteEvent()
                && PluginConfig.SpriteSubRowsEnabled)
            {
                float delta = Holder.GetSpriteEventExtraOffset(control.levelEvent);
                transform.AnchorRelativeY(-delta);
            }
        }

        public void UpdateSpriteHeaders(TabSection_Sprites tab)
        {
            if (!PluginConfig.SpriteSubRowsEnabled)
            {
                return;
            }

            float extraOffset = 0;

            List<LevelEvent_MakeSprite> currentSprites = tab.editor.currentPageSpritesData;
            for (int i = 0; i < tab.spriteHeaders.Count - 1; i++)
            {
                LayoutElement element = tab.spriteHeaders[i].EnsureComponent<LayoutElement>();
                element.layoutPriority = 99;

                int extraRowCount = GetSpriteExtraRowCount(currentSprites[i].spriteId);

                if (extraRowCount > 0)
                {
                    Image image = tab.spriteHeaders[i].GetComponent<Image>();

                    float heightOffset = tab.editor.cellHeight * extraRowCount;

                    element.preferredHeight = image.preferredHeight + heightOffset;
                    extraOffset += heightOffset;

                    element.enabled = true;
                } else
                {
                    element.enabled = false;
                }
            }

            tab.headersListRect.SizeDeltaY(tab.spriteHeaders.Count * tab.editor.cellHeight + extraOffset);
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
                        maxUsedY += Holder.GetSpriteExtraRowCount(sprite.spriteId) + 1;
                    }
                    break;
                default:
                    break;
            }

            editor.timeline.maxUsedY = maxUsedY;
        }

        private float GetSpriteEventExtraOffset(LevelEvent_Base levelEvent)
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

        private int GetSpriteExtraRowCount(string target)
        {
            if (spriteData.TryGetValue(target, out PageInfo info))
            {
                return info.usedSubRowCount;
            }

            return 0;
        }

        public bool TryFindSpriteForRow(int row, out string id, out int subRow)
        {
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
            }

            id = null;
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
                default:
                    break;
            }
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

            UpdateSpriteHeaders(scnEditor.instance.tabSection_sprites);
        }

        public class EventInfo(int subRow)
        {
            public int subRow = subRow;
        }

        public class PageInfo()
        {
            public int usedSubRowCount = 0;
        }

        public const string SubRowKey = "mod_rdEditorPlus_subRow";
        public const string InternalHeaderSubRowKey = "_mod_rdEditorPlus_headerSubRow";
    }
}
