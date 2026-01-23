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
    }
}
