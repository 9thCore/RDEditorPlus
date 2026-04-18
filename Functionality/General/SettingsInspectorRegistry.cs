using RDEditorPlus.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.General
{
    public static class SettingsInspectorRegistry
    {
        public static Action OnUpdateUI;
        public static List<BaseOption> Settings { get; } = [];

        public static void Register(params BaseOption[] instances) => Settings.AddRange(instances);

        public class DropdownOption : SimpleOption
        {
            public DropdownOption(string name, List<string> options, UnityAction<int> onValueChanged) : base(name)
            {
                ElementOffset = -0.5f;
                this.options = options;
                this.onValueChanged = onValueChanged;
            }

            public Dropdown Dropdown { get; private set; }

            protected override void SetupRectTransform(Transform parent)
            {
                UnityUtil.CreateDropdown(parent, out var dropdown, out var dropdownRT);

                dropdown.ClearOptions();
                dropdown.AddOptions(options);
                dropdown.onValueChanged.AddListener(onValueChanged);

                Dropdown = dropdown;
                RectTransform = dropdownRT;
            }

            private readonly UnityAction<int> onValueChanged;
            private readonly List<string> options;
        }

        public class ButtonOption : SimpleOption
        {
            public ButtonOption(string name, Color color, ColorBlock colors, UnityAction onClick) : base(name)
            {
                ElementOffset = 17f;
                Color = color;
                Colors = colors;
                OnClick = onClick;
            }

            public ButtonOption(string name, UnityAction onClick) : this(name, DefaultColor, DefaultColors, onClick) { }

            protected override void SetupRectTransform(Transform parent)
            {
                GameObject buttonGO = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_Button{Name}");

                var buttonImage = buttonGO.AddComponent<Image>();
                buttonImage.sprite = AssetUtil.ButtonSprite;
                buttonImage.type = Image.Type.Tiled;
                buttonImage.color = Color;

                var button = buttonGO.AddComponent<Button>();
                button.onClick.AddListener(OnClick);
                button.colors = Colors;

                var buttonRT = buttonGO.transform as RectTransform;

                GameObject buttonTextGO = new("text");

                var buttonText = buttonTextGO.AddComponent<Text>();
                buttonText.text = "Setup...";
                buttonText.ApplyRDFont();
                buttonText.alignment = TextAnchor.MiddleLeft;

                var buttonTextRT = buttonTextGO.transform as RectTransform;
                buttonTextRT.SetParent(buttonRT, worldPositionStays: false);
                buttonTextRT.anchorMin = Vector2.zero;
                buttonTextRT.anchorMax = Vector2.one;
                buttonTextRT.offsetMin = new Vector2(2f, 2f);
                buttonTextRT.offsetMax = new Vector2(-2f, -2f);

                RectTransform = buttonRT;
                buttonRT.SetParent(parent, worldPositionStays: false);
            }

            private UnityAction OnClick { get; init; }
            private Color Color { get; init; }
            private ColorBlock Colors { get; init; }

            private static readonly Color DefaultColor = Color.gray;

            private static readonly ColorBlock DefaultColors = new()
            {
                normalColor = "FFFFFFFF".HexToColor(),
                highlightedColor = "D5D5D5FF".HexToColor(),
                pressedColor = "A8A8A8FF".HexToColor(),
                selectedColor = "D5D5D5FF".HexToColor(),
                disabledColor = "C8C8C880".HexToColor(),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
        }

        public abstract class SimpleOption(string name) : BaseOption(name)
        {
            public RectTransform RectTransform { get; protected set; }
            public float ElementOffset { get; init; } = 0f;

            public override void Setup(Transform parent, Vector2 offsetMin, Vector2 offsetMax, float anchorPosY)
            {
                SetupRectTransform(parent);

                RectTransform.anchorMin = Vector2.zero;
                RectTransform.anchorMax = Vector2.one;
                RectTransform.offsetMin = offsetMin;
                RectTransform.offsetMax = offsetMax;
                RectTransform.AnchorPosY(anchorPosY + ElementOffset);
            }

            protected abstract void SetupRectTransform(Transform parent);
        }

        public abstract class BaseOption(string name)
        {
            public string Name { get; init; } = name;

            public abstract void Setup(Transform parent, Vector2 offsetMin, Vector2 offsetMax, float anchorPosY);
        }
    }
}
