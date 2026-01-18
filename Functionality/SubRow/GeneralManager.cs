using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        public override int? GetCurrentTabMaxUsedY()
        {
            return currentTabController?.GetCurrentTabMaxUsedY();
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
            rowController.Clear();
            windowController.Clear();

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

        public override int? GetTimelineDisabledRowsValueThing()
        {
            return currentTabController?.GetTimelineDisabledRowsValueThing();
        }

        public override bool AffectedByTallEventConfig()
        {
            throw new InvalidOperationException();
        }

        public bool CanAllSelectedEventsBeDragged(bool originalFlag, int offset)
        {
            return currentTabController?.CanAllSelectedEventsBeDragged(offset) ?? originalFlag;
        }

        public int ModifyPointerClickYPosition(int y)
        {
            if (currentTabController == null
                || !currentTabController.TryGetPreCreationEventData(y, out int realY, out int _, out int visualRow))
            {
                return y;
            }

            preCreationEventVisualRow = visualRow;
            return realY;
        }
        
        public int RowEventYFix(int y, LevelEventControl_Base eventControl)
        {
            if (currentTabController == null
                || !currentTabController.TryGetPreCreationEventData(y, out int realY, out int _, out int _))
            {
                return y;
            }

            SubRowStorage.Instance.SetVisualRow(eventControl.levelEvent, y);
            UpdateTab(force: false);
            scnEditor.instance.timeline.UpdateMaxUsedY();
            return realY;
        }

        public void ChangeTab(Tab tab)
        {
            currentTabController = tab switch
            {
                Tab.Sprites => PluginConfig.SpriteSubRowsEnabled ? spriteController : null,
                Tab.Rooms => PluginConfig.RoomSubRowsEnabled ? roomController : null,
                Tab.Windows => PluginConfig.WindowSubRowsEnabled ? windowController : null,
                Tab.Rows => PluginConfig.PatientSubRowsEnabled ? rowController : null,
                _ => null,
            };

            if (tab != Tab.Sprites)
            {
                ResetAlternatingTimelineStrips();
                UpdateTab(force: false);
            }
        }

        public void ResetAlternatingTimelineStrips()
        {
            foreach (RectTransform rect in alternatingTimelineStrip)
            {
                rect.gameObject.SetActive(false);
            }
        }

        public void SetAlternatingTimelineStrips(List<int> rows)
        {
            if (!PluginConfig.AlternatingColorSubRowsEnabled)
            {
                return;
            }

            float offset = 0f;

            if (PluginConfig.TallEventSubRowsBehaviour == PluginConfig.SubRowTallEventBehaviour.KeepInSpecialRow
                && currentTabController != null
                && currentTabController.AffectedByTallEventConfig())
            {
                offset = -scnEditor.instance.cellHeight;
            }

            lastZoom = scnEditor.instance.timeline.zoomVertFactor;
            float length = 0f;

            for (int i = alternatingTimelineStrip.Count * 2; i < rows.Count; i += 2)
            {
                GameObject strip = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_AlternatingTimelineStrip{alternatingTimelineStrip.Count}");
                strip.layer = RDLayer.UI;

                if (alternatingTimelineImageTemplate == null)
                {
                    alternatingTimelineImageTemplate = scnEditor.instance.timeline.scrollViewVertContent
                        .GetComponentInParent<Image>();
                }

                strip.transform.SetParent(scnEditor.instance.timeline.scrollViewVertContent);
                strip.transform.SetAsFirstSibling();
                strip.transform.localPosition = Vector3.zero;
                strip.transform.localScale = Vector3.one;

                Image image = strip.AddComponent<Image>();
                image.sprite = alternatingTimelineImageTemplate.sprite;
                image.type = alternatingTimelineImageTemplate.type;
                image.color = alternateColor;
                image.raycastTarget = false;

                RectTransform rect = strip.GetComponent<RectTransform>();
                alternatingTimelineStrip.Add(rect);

                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;

                if (length == 0f)
                {
                    length = scnEditor.instance.timeline
                        .grid.GetComponent<RectTransform>()
                        .offsetMax.x;
                }

                rect.offsetMax = new Vector2(length, 0f);
            }

            int index = 0;

            for (int i = 0; i < rows.Count; i += 2)
            {
                RectTransform rect = alternatingTimelineStrip[index++];
                rect.gameObject.SetActive(true);

                float height = rows[i] * scnEditor.instance.cellHeight;

                rect.OffsetMaxY(offset);
                offset -= height;
                rect.offsetMin = new Vector2(0f, offset);

                if (i < rows.Count - 1)
                {
                    offset -= rows[i + 1] * scnEditor.instance.cellHeight;
                }
            }

            for (; index < alternatingTimelineStrip.Count; index++)
            {
                alternatingTimelineStrip[index].gameObject.SetActive(false);
            }
        }

        public void ResizeAlternatingTimelineStrips()
        {
            float length = scnEditor.instance.timeline.grid.GetComponent<RectTransform>().offsetMax.x;
            float zoom = scnEditor.instance.timeline.zoomVertFactor;
            float factor = zoom / lastZoom;
            lastZoom = zoom;

            foreach (RectTransform rect in alternatingTimelineStrip)
            {
                rect.OffsetMinY(rect.offsetMin.y * factor);
                rect.offsetMax = new Vector2(length, rect.offsetMax.y * factor);
            }
        }

        public int PreCreationEventVisualRow => preCreationEventVisualRow;

        private int preCreationEventVisualRow = 0;

        private float lastZoom;

        private BaseManager currentTabController = null;
        private readonly SpriteManager spriteController = SpriteManager.Instance;
        private readonly RoomManager roomController = RoomManager.Instance;
        private readonly WindowManager windowController = WindowManager.Instance;
        private readonly RowManager rowController = RowManager.Instance;

        private Image alternatingTimelineImageTemplate = null;
        private readonly List<RectTransform> alternatingTimelineStrip = new();

        public static readonly Color alternateColor = new(1f, 1f, 1f, 0.1f);
    }
}
