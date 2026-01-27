using RDLevelEditor;
using System.Collections.Generic;
using System.Collections.Immutable;
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

            // Check if all events are of the same type
            LevelEventType type = levelEvent.type;
            if (!selectedControls.All(control => control.levelEvent.type == type))
            {
                return false;
            }

            // Check if all properties which need enabling are enabled or disabled for all events in common
            // Still unsure about this, I might end up going for specific checks for each event type in particular
            ImmutableList<BasePropertyInfo> properties = levelEvent.info.propertiesInfo;
            foreach (LevelEventControl_Base control in selectedControls)
            {
                LevelEvent_Base secondLevelEvent = control.levelEvent;
                if (secondLevelEvent == levelEvent)
                {
                    continue;
                }

                if (properties.Any(property => property.enableIf != null &&
                property.enableIf(levelEvent) != property.enableIf(secondLevelEvent)))
                {
                    return false;
                }
            }

            return true;
        }

        public static void SetupMixedText(Text text, string mixedText = null)
        {
            text.text = mixedText ?? MixedText;
            text.color = Color.black.WithAlpha(0.3f);
        }

        public static void SetupMixedPlaceholder(this InputField inputField, string mixedText = null)
        {
            if (inputField.placeholder != null)
            {
                return;
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

        public const string DefaultNullText = "--";
        public const string MixedText = "[mixed]";
        public const string MixedTextShorter = "[mix]";
        public const string MixedTextSliderPercent = "[mix]";
    }
}
