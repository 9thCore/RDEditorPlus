using RDEditorPlus.Functionality.SubRow;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.Windows
{
    public class MoreWindowManager : MonoBehaviour
    {
        private static MoreWindowManager instance;
        public static MoreWindowManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject manager = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_MoreWindowsManager");
                    instance = manager.AddComponent<MoreWindowManager>();
                }

                return instance;
            }
        }

        public void AddWindow()
        {
            using (new SaveStateScope(clearRedo: true, skipSaving: false, skipTimelinePos: false))
            {
                foreach (var control in scnEditor.instance.eventControls_windows)
                {
                    if (control.levelEvent is LevelEvent_ReorderWindows levelEvent)
                    {
                        int[] order = levelEvent.order;
                        levelEvent.order = new int[levelEvent.order.Length + 1];

                        for (int i = order.Length - 1; i >= 0; i--)
                        {
                            levelEvent.order[i] = order[i];
                        }
                        levelEvent.order[order.Length] = WindowCount;
                    }
                }
            }

            int value = extraWindowCount + 1;
            UpdateTabListSize(extraWindowCount, value);
            extraWindowCount = value;

            UpdateTabWindows(WindowCount);

            if (PluginConfig.SubRowsEnabled && PluginConfig.WindowSubRowsEnabled)
            {
                scnEditor.instance.timeline.UpdateMaxUsedY();
                WindowManager.Instance.UpdateTab(force: false);
            }
            
            scnEditor.instance.timeline.UpdateUI();
        }

        public void RemoveWindow(int index)
        {
            using (new SaveStateScope(clearRedo: true, skipSaving: false, skipTimelinePos: false))
            {
                List<LevelEventControl_Base> toBeDeleted = new();

                foreach (var control in scnEditor.instance.eventControls_windows)
                {
                    if (control.levelEvent.y == index)
                    {
                        toBeDeleted.Add(control);
                    }
                    else
                    {
                        if (control.levelEvent.y > index)
                        {
                            control.levelEvent.y--;
                            control.UpdateUI();
                        }

                        if (control.levelEvent is LevelEvent_ReorderWindows levelEvent)
                        {
                            levelEvent.order = levelEvent.order.Where(windowIndex => windowIndex != WindowCount - 1).ToArray();
                        }
                    }
                }

                scnEditor.instance.DeleteEventControlsFromList(toBeDeleted, sound: false);
            }

            int value = extraWindowCount - 1;
            UpdateTabListSize(extraWindowCount, value);
            extraWindowCount = value;

            UpdateTabWindows(WindowCount);

            if (PluginConfig.SubRowsEnabled && PluginConfig.WindowSubRowsEnabled)
            {
                scnEditor.instance.timeline.UpdateMaxUsedY();
                WindowManager.Instance.UpdateTab(force: false);
            }

            scnEditor.instance.timeline.UpdateUI();
        }

        public void SetActiveExtraWindows(int count)
        {
            int value = Math.Max(0, count);
            UpdateTabListSize(extraWindowCount, value);
            extraWindowCount = value;

            UpdateTabWindows(WindowCount);
        }

        public void DecodeModData(Dictionary<string, object> data)
        {
            if (data == null)
            {
                SetActiveExtraWindows(0);
                return;
            }

            if (data.TryGetValue(ExtraWindowCountKey, out var value) && value is int count)
            {
                SetActiveExtraWindows(count);
            }
            else
            {
                SetActiveExtraWindows(0);
            }
        }

        public bool TryConstructJSONData(out string data)
        {
            if (extraWindowCount == 0)
            {
                data = default;
                return false;
            }

            data = $"\"{ExtraWindowCountKey}\": {extraWindowCount}";
            return true;
        }

        public int WindowCount => RDEditorConstants.WindowCount + extraWindowCount;
        public int ExtraWindowCount => extraWindowCount;

        public const string ExtraWindowCountKey = "extraWindowCount";

        private static void UpdateTabWindows(int windowCount)
        {
            var tab = scnEditor.instance.tabSection_windows;
            DuplicateWindowsIfRequired(tab, windowCount);

            Transform group = tab.listRect.GetComponentInChildren<VerticalLayoutGroup>().transform;
            Transform addButton = group.GetChild(group.childCount - 1);

            int num = 0;
            foreach (Transform child in group)
            {
                if (child != addButton)
                {
                    child.gameObject.SetActive(num < windowCount);
                }

                num++;
            }
        }

        private static void UpdateTabListSize(int oldExtraWindowCount, int extraWindowCount)
        {
            var rect = scnEditor.instance.tabSection_windows.listRect;
            rect.OffsetMinY(rect.offsetMin.y - scnEditor.instance.cellHeight * (extraWindowCount - oldExtraWindowCount));
        }

        private static void DuplicateWindowsIfRequired(TabSection_Windows tab, int desiredWindowCount)
        {
            if (tab.labels.Length >= desiredWindowCount)
            {
                return;
            }

            GameObject template = tab.labels[0].gameObject;
            Transform parent = template.transform.parent;
            Transform currentLastSibling = parent.GetChild(parent.childCount - 1);

            List<Text> labels = tab.labels.ToList();
            List<RawImage> previews = tab.previews.ToList();
            List<Image> previewBorders = tab.previewBorders.ToList();

            for (int i = tab.labels.Length; i < desiredWindowCount; i++)
            {
                GameObject clone = GameObject.Instantiate(template);

                clone.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_Window{i}";

                clone.transform.SetParent(parent);
                clone.transform.localScale = Vector3.one;

                clone.transform.SetAsLastSibling();

                var text = clone.GetComponent<Text>();
                text.text = RDString.Get("editor.windowIndex").Replace("[index]", (i + 1).ToString());
                text.horizontalOverflow = HorizontalWrapMode.Overflow; // the text might be hidden by the preview but whateverrr

                int index = i;
                clone.AddComponent<Button>().onClick.AddListener(() =>
                {
                    Instance.RemoveWindow(index);
                });

                var border = clone.transform.Find("previewBorder").GetComponent<Image>();

                labels.Add(text);
                previews.Add(border.transform.Find("preview0").GetComponent<RawImage>());
                previewBorders.Add(border);
            }

            tab.labels = labels.ToArray();
            tab.previews = previews.ToArray();
            tab.previewBorders = previewBorders.ToArray();

            currentLastSibling.SetAsLastSibling();
        }

        private int extraWindowCount = 0;
    }
}
