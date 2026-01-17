using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.SubRow
{
    public class SpriteManager : BaseManager
    {
        private static SpriteManager instance;
        public static SpriteManager Instance
        {
            get
            {
                instance ??= new SpriteManager();
                return instance;
            }
        }

        public override void UpdateFullTimelineHeightEvent(LevelEventControl_Base eventControl)
        {
            // There should be no full timeline height events in the sprites tab
            throw new InvalidOperationException();
        }

        public override bool CanAllSelectedEventsBeDragged(int offset)
        {
            foreach (LevelEventControl_Base control in scnEditor.instance.selectedControls)
            {
                int row = SubRowStorage.Instance.GetEventCorrectedRow(control.levelEvent);
                int newRow = row + offset;

                if (!TryFindSpriteForRow(newRow, out _, out _))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetDraggedEventYPosition(LevelEventControl_Base eventControl, int oldY)
        {
            if (!TryFindSpriteForRow(oldY, out string id, out int roomPosition))
            {
                return -(roomPosition - 1);
            }

            if (!eventControl.levelEvent.IsPreCreationEvent())
            {
                SubRowStorage.Instance.SetLevelEventTarget(eventControl, id);
            }

            return roomPosition;
        }

        public override void UpdateTab(bool force)
        {
            UpdatePage(force, scnEditor.instance.selectedSpritesTabPageIndex);
            UpdateTabPanelOnly();
        }

        public override void UpdateTabPanelOnly()
        {
            float extraOffset = 0f;

            TabSection_Sprites tab = scnEditor.instance.tabSection_sprites;
            List<LevelEvent_MakeSprite> currentSprites = scnEditor.instance.currentPageSpritesData;
            for (int i = 0; i < tab.spriteHeaders.Count - 1; i++)
            {
                extraOffset += ResizeHeader(tab.spriteHeaders[i],
                    SubRowStorage.Instance.GetSpriteExtraVisualRowCount(currentSprites[i].spriteId));
            }

            tab.headersListRect.SizeDeltaY(tab.spriteHeaders.Count * scnEditor.instance.cellHeight + extraOffset);
        }

        public override int? GetCurrentTabMaxUsedY()
        {
            int maxUsedY = 0;

            List<LevelEvent_MakeSprite> currentSprites = scnEditor.instance.currentPageSpritesData;
            foreach (LevelEvent_MakeSprite sprite in currentSprites)
            {
                maxUsedY += SubRowStorage.Instance.GetSpriteVisualRowCount(sprite.spriteId);
            }

            return maxUsedY;
        }

        public override bool TryGetPreCreationEventData(int y, out int realY, out int realRow, out int visualRow)
        {
            if (!TryFindSpriteForRow(y, out string spriteID, out int roomPosition))
            {
                realY = 0;
                realRow = 0;
                visualRow = 0;
                return false;
            }

            realRow = SpriteHeader.GetSpriteDataIndex(spriteID);
            realY = roomPosition;
            visualRow = y;
            return true;
        }

        public override void SetupJustCreatedEvent(LevelEvent_Base levelEvent)
        {
            levelEvent.target = SpriteHeader.GetSpriteData(levelEvent.y, levelEvent.room).spriteId;
        }

        public override int? GetTimelineDisabledRowsValueThing()
        {
            return Mathf.Min(scnEditor.instance.timeline.maxUsedY, scnEditor.instance.timeline.scaledRowCellCount - 2);
        }

        private void UpdatePage(bool force, int page)
        {
            // If an event is above the first decoration that got its size updated,
            // there's no point updating it as it definitely did not move.
            // ...Except if we're to force doing it anyway, as part of an undo
            bool updateEventPositions = force;

            List<int> alternatingRows = new();

            List<LevelEvent_MakeSprite> currentSprites = scnEditor.instance.spritesData;
            foreach (LevelEvent_MakeSprite sprite in currentSprites)
            {
                if (sprite.room != page)
                {
                    continue;
                }

                int usedSubRowCount = 0;

                int index = SpriteHeader.GetSpriteDataIndex(sprite.spriteId);

                List<LevelEventControl_Base> controls = scnEditor.instance.eventControls_sprites[index];
                foreach (LevelEventControl_Base control in controls)
                {
                    if (control.levelEvent.IsPreCreationEvent())
                    {
                        continue;
                    }

                    int num = SubRowStorage.Instance.GetSubRow(control.levelEvent) + 1;
                    usedSubRowCount = Math.Max(usedSubRowCount, num);

                    if (updateEventPositions)
                    {
                        control.UpdateUI();
                    }
                }

                updateEventPositions |= SubRowStorage.Instance.UpdateSpriteUsedSubRowCountIfRequired(sprite.spriteId, usedSubRowCount);

                alternatingRows.Add(usedSubRowCount + 1);
            }

            GeneralManager.Instance.SetAlternatingTimelineStrips(alternatingRows);
        }

        private float ResizeHeader(GameObject header, int extraRowCount)
        {
            LayoutElement element = header.EnsureComponent<LayoutElement>();
            element.layoutPriority = SubRowStorage.LayoutElementPriority;

            if (extraRowCount > 0)
            {
                Image image = header.GetComponent<Image>();

                float heightOffset = scnEditor.instance.cellHeight * extraRowCount;

                element.preferredHeight = image.preferredHeight + heightOffset;

                element.enabled = true;

                return heightOffset;
            }

            element.enabled = false;
            return 0f;
        }

        private bool TryFindSpriteForRow(int row, out string id, out int roomPosition)
        {
            return TryFindSpriteForRow(scnEditor.instance.selectedSpritesTabPageIndex, row, out id, out roomPosition);
        }

        private bool TryFindSpriteForRow(int room, int row, out string id, out int roomPosition)
        {
            roomPosition = 0;

            if (row < 0)
            {
                id = null;
                return false;
            }

            foreach (LevelEvent_MakeSprite sprite in scnEditor.instance.spritesData)
            {
                if (sprite.room != room)
                {
                    continue;
                }

                int rowCount = SubRowStorage.Instance.GetSpriteVisualRowCount(sprite.spriteId);
                row -= rowCount;

                if (row < 0)
                {
                    id = sprite.spriteId;
                    return true;
                }

                roomPosition++;
            }

            id = null;
            return false;
        }
    }
}
