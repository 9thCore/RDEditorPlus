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
            if (!PluginConfig.SelectionMultiEditEnabled
                || scnEditor.instance == null)
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

        public static void SetupMixedText(Text text)
        {
            text.text = MixedText;
            text.color = Color.black.WithAlpha(0.3f);
        }

        public const string MixedText = "[mixed]";
    }
}
