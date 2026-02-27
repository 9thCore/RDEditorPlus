using RDEditorPlus.Functionality.SubRow;
using RDEditorPlus.Util;
using RDLevelEditor;
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
            extraWindowCount++;
            UpdateTabWindows(WindowCount);

            scnEditor.instance.timeline.UpdateMaxUsedY();

            if (PluginConfig.SubRowsEnabled && PluginConfig.WindowSubRowsEnabled)
            {
                WindowManager.Instance.UpdateTab(force: false);
            }
            
            scnEditor.instance.timeline.UpdateUI();
        }

        public void RemoveWindow(int index)
        {
            extraWindowCount--;
            UpdateTabWindows(WindowCount);
        }

        public int WindowCount => RDEditorConstants.WindowCount + extraWindowCount;
        public int ExtraWindowCount => extraWindowCount;

        private static void UpdateTabWindows(int windowCount)
        {
            var tab = scnEditor.instance.tabSection_windows;
            DuplicateWindowsIfRequired(tab, windowCount);
        }

        private static void DuplicateWindowsIfRequired(TabSection_Windows tab, int desiredWindowCount)
        {
            if (tab.labels.Length >= desiredWindowCount)
            {
                return;
            }

            int toBeAdded = desiredWindowCount - tab.labels.Length;

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

                var border = clone.transform.Find("previewBorder").GetComponent<Image>();

                labels.Add(text);
                previews.Add(border.transform.Find("preview0").GetComponent<RawImage>());
                previewBorders.Add(border);
            }

            tab.labels = labels.ToArray();
            tab.previews = previews.ToArray();
            tab.previewBorders = previewBorders.ToArray();

            currentLastSibling.SetAsLastSibling();

            var rect = scnEditor.instance.tabSection_windows.listRect;
            rect.OffsetMinY(rect.offsetMin.y - scnEditor.instance.cellHeight * toBeAdded);
        }

        private int extraWindowCount = 0;
    }
}
