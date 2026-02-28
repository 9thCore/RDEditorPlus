using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.Windows
{
    public sealed class ReorderWindowStorage : MonoBehaviour
    {
        public void UpdateUI(LevelEventControl_Window eventControl)
        {
            var levelEvent = eventControl.levelEvent as LevelEvent_ReorderWindows;
            int orderCount = levelEvent.order.Length;

            EnsureEnoughText(eventControl, orderCount);

            int index = 0;
            foreach (var text in order)
            {
                if (index >= orderCount)
                {
                    text.gameObject.SetActive(false);
                }
                else
                {
                    text.gameObject.SetActive(true);
                    int placement = Array.IndexOf(levelEvent.order, index);

                    if (placement == -1)
                    {
                        text.text = "#?";
                    }
                    else
                    {
                        text.text = $"#{placement + 1}";
                    }
                }

                index++;
            }
        }

        public Text TextForWindow(int index) => order[index];

        private void EnsureEnoughText(LevelEventControl_Window eventControl, int windowCount)
        {
            for (int i = order.Count; i < windowCount; i++)
            {
                order.Add(CreateText());
            }
        }

        private Text CreateText()
        {
            GameObject clone = Instantiate(template);
            clone.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_Order";

            var text = clone.GetComponent<Text>();
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.alignment = TextAnchor.MiddleCenter;

            clone.transform.SetParent(parent);
            clone.transform.localScale = Vector3.one;
            clone.transform.localPosition = Vector3.zero;
            clone.transform.SetAsLastSibling();

            RectTransform rt = clone.transform as RectTransform;
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            clone.SetActive(true);
            return text;
        }

        private void Awake()
        {
            if (TryGetComponent(out LevelEventControl_Window eventControl))
            {
                parent = eventControl.orderIcons[0].transform.parent;
                parent.EnsureComponent<VerticalLayoutGroup>();
            }

            if (template == null)
            {
                template = Instantiate(scnEditor.instance.tabSection_windows.labels[0].gameObject);
                template.SetActive(false);

                DestroyImmediate(template.transform.GetChild(0).gameObject);
                DestroyImmediate(template.GetComponent<RDEventTrigger>());

                template.GetComponent<EightSidedOutline>().effectColor = Color.black.WithAlpha(0.5f);

                RectTransform rt = template.transform as RectTransform;

                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
            }
        }

        private readonly List<Text> order = new();
        private Transform parent;
        private static GameObject template;
    }
}
