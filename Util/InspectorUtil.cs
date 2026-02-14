using RDLevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Util
{
    public static class InspectorUtil
    {
        public static Button CopyButton(Transform template, string name, string text)
        {
            if (template == null)
            {
                return null;
            }

            GameObject copy = GameObject.Instantiate(template.gameObject);
            copy.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_{name}";

            copy.transform.SetParent(template.transform.parent);
            copy.transform.SetAsLastSibling();

            copy.transform.localScale = Vector3.one;

            copy.gameObject.GetComponentInChildren<Text>().text = text;

            return copy.gameObject.GetComponentInChildren<Button>();
        }

        public static bool CanMultiEdit()
        {
            if (scnEditor.instance == null)
            {
                return false;
            }

            List<LevelEventControl_Base> selectedControls = scnEditor.instance.selectedControls;

            if (selectedControls.Count <= 1)
            {
                return false;
            }

            LevelEvent_Base levelEvent = selectedControls[0].levelEvent;

            if (!levelEvent.inspectorPanel.auto)
            {
                return false;
            }

            LevelEventType type = levelEvent.type;
            if (selectedControls.Any(control => control.levelEvent.isBaseEvent || control.levelEvent.type != type))
            {
                return false;
            }

            return true;
        }

        public static void SetupMixedText(Text text, string mixedText = null)
        {
            text.text = mixedText ?? MixedText;
            text.color = Color.black.WithAlpha(MixedTextAlpha);
        }

        public static Text SetupMixedPlaceholder(this InputField inputField, string mixedText = null)
        {
            if (inputField.placeholder != null)
            {
                return inputField.placeholder as Text;
            }

            RectTransform template = (RectTransform) inputField.textComponent.transform;

            GameObject instance = Object.Instantiate(template.gameObject);
            instance.SetActive(false);

            RectTransform transform = (RectTransform)instance.transform;

            transform.SetParent(inputField.transform);
            transform.localRotation = template.localRotation;
            transform.localScale = template.localScale;
            transform.offsetMin = template.offsetMin;
            transform.offsetMax = template.offsetMax;

            Text text = instance.GetComponent<Text>();
            SetupMixedText(text, mixedText);
            inputField.placeholder = text;

            instance.SetActive(true);

            return text;
        }

        public static bool AcceptsNull(this PropertyControl_InputField propertyControl)
        {
            return propertyControl.inputField == propertyControl.expInputField;
        }

        public static bool IsUsedMultiEdit(this NullablePropertyInfo nullablePropertyInfo)
        {
            if (!CanMultiEdit())
            {
                return false;
            }

            return scnEditor.instance.selectedControls.All(eventControl => nullablePropertyInfo.propertyInfo.GetValue(eventControl.levelEvent) != null);
        }

        public static bool RowEqualValueForSelectedEvents(this InspectorPanel panel)
        {
            if (!CanMultiEdit())
            {
                return true;
            }

            LevelEventInfo info = panel.levelEventInfo;

            if (!info.showsRowControl)
            {
                return true;
            }

            int row = scnEditor.instance.selectedControls[0].levelEvent.row;
            return scnEditor.instance.selectedControls.All(control => control.levelEvent.row == row);
        }

        public static bool TryMultiEditUpdateUI(this InspectorPanel panel)
        {
            if (!CanMultiEdit())
            {
                panel.position.ResetTagRunGraphics(scnEditor.instance.selectedControls[0].levelEvent.tagRunNormally);
                return true;
            }

            panel.position.MultiEditUpdateUI();
            return false;
        }

        public static void MultiEditUpdateUI(this BarBeatPosition position)
        {
            var controls = scnEditor.instance.selectedControls;
            LevelEventControl_Base eventControl = scnEditor.instance.selectedControls[0];

            int bar = eventControl.bar;
            string beat = eventControl.beat.ToString();
            string tag = eventControl.levelEvent.tag;
            bool tagRunNormally = eventControl.levelEvent.tagRunNormally;

            if (controls.Any(eventControl => eventControl.bar != bar))
            {
                ((Text)position.bar.placeholder).text = MixedTextBar;
                position.bar.text = string.Empty;
            }

            if (controls.Any(eventControl => eventControl.beat.ToString() != beat))
            {
                ((Text)position.beat.placeholder).text = MixedTextBeat;
                position.beat.text = string.Empty;
            }

            if (controls.Any(eventControl => eventControl.levelEvent.tag != tag))
            {
                ((Text)position.evTag.placeholder).text = MixedTextTag;
                position.evTag.text = string.Empty;
            }

            if (controls.Any(eventControl => eventControl.levelEvent.tagRunNormally != tagRunNormally))
            {
                position.SetMixedTagRunGraphics();
            }
            else
            {
                position.ResetTagRunGraphics(tagRunNormally);
            }

            position.UpdateUI();
        }

        private static void SetMixedTagRunGraphics(this BarBeatPosition position)
        {
            position.evTagRunToggle.SetIsOnWithoutNotify(false);

            position.evTagRunToggle.transform.Find(MixedTagRunButtonGraphicPath).gameObject.SetActive(true);

            if (position.evTagRunToggle.TryGetComponentInParent(out PropertyControl_Checkbox checkbox))
            {
                checkbox.ShowDisabledIcon(active: true);
            }
        }

        private static void ResetTagRunGraphics(this BarBeatPosition position, bool tagRunNormally)
        {
            position.evTagRunToggle.transform.Find(MixedTagRunButtonGraphicPath).gameObject.SetActive(false);

            if (position.evTagRunToggle.TryGetComponentInParent(out PropertyControl_Checkbox checkbox))
            {
                checkbox.ShowDisabledIcon(tagRunNormally);
            }
        }

        public const float MixedTextAlpha = 0.3f;
        public const float MixedSliderAlpha = 0.5f;
        public const float MixedCheckboxAlpha = 0.3f;
        public const float MixedColorKnobAlpha = 0.5f;
        public const float MixedTagRunButtonAlpha = 0.3f;
        public const string DefaultNullText = "--";
        public const string MixedText = "[mixed]";
        public const string MixedTextShorter = "[mix]";
        public const string MixedTextSliderPercent = "[mix]";
        public const string MixedTextBar = "mix";
        public const string MixedTextBeat = "[mix]";
        public const string MixedTextTag = "[multiple]";
        public const string MixedTagRunButtonGraphic = $"Mod_{MyPluginInfo.PLUGIN_GUID}_MixedGraphic";
        public const string MixedTagRunButtonGraphicPath = $"Background/{MixedTagRunButtonGraphic}";

        public static readonly Color ConditionalNormalColor = new(4f / 255f, 1f, 1f);
        public static readonly Color ConditionalNegatedColor = new(1f, 4f / 255f, 4f / 255f);
        public static readonly Color ConditionalMixedColor = new(1f, 1f, 4f / 255f);
        public static readonly Color ConditionalUnusedColor = new(1f, 1f, 1f);

        public static readonly Color RoomUnusedColor = "5B5B5BFF".HexToColor();
        public static Color RoomUsedColor => RDConstants.data.colorPalette[3];
        public static Color RoomHalfUsedColor => Color.Lerp(RoomUnusedColor, RoomUsedColor, 0.5f);
    }
}
