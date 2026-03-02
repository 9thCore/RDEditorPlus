using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes
{
    public class Node : MonoBehaviour
    {
        public static GameObject PreparePrefab(string name, IEnumerable<InputData> inputs, IEnumerable<OutputData> outputs)
        {
            GameObject root = Instantiate(BaseNode);
            root.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_Node{name}";

            var node = root.GetComponent<Node>();
            node.SetName(name);

            return root;
        }

        public void SetName(string name)
        {
            title.text = name;
        }

        public void Drag(Vector2 delta)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            rectTransform.anchoredPosition += delta / rectTransform.lossyScale;
        }

        public readonly struct InputData(Type type, string name)
        {
            public readonly Type type = type;
            public readonly string name = name;
        }

        public readonly struct OutputData(Type type, string name)
        {
            public readonly Type type = type;
            public readonly string name = name;
        }

        public enum Type
        {
            Float
        }

        [SerializeField]
        private Text title;

        [SerializeField]
        private RectTransform rectTransform;

        public static Font Font;
        public static Sprite Sprite;

        private static GameObject BaseNode
        {
            get
            {
                if (baseNode == null)
                {
                    baseNode = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_NodeBase");
                    baseNode.SetActive(false);

                    baseNode.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);

                    var image = baseNode.AddComponent<Image>();
                    image.sprite = Sprite;
                    image.color = Color.gray.WithBrightness(0.8f);
                    image.type = Image.Type.Tiled;

                    GameObject title = new("title");
                    title.transform.SetParent(baseNode.transform);
                    title.transform.localPosition = Vector3.zero;
                    title.transform.localScale = Vector3.one;

                    var node = baseNode.AddComponent<Node>();

                    node.title = title.AddComponent<Text>();
                    node.title.font = Font;
                    node.title.alignment = TextAnchor.UpperCenter;
                    node.title.fontSize = 8;

                    title.AddComponent<EightSidedOutline>().effectColor = Color.black;

                    var titleRT = node.title.rectTransform;
                    titleRT.anchorMin = Vector2.zero;
                    titleRT.anchorMax = new Vector2(1f, 0.94f);
                    titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

                    var rt = baseNode.transform as RectTransform;
                    rt.offsetMin = new Vector2(-50f, -30f);
                    rt.offsetMax = -rt.offsetMin;

                    node.rectTransform = rt;
                    baseNode.AddComponent<NodeEventTrigger>().Setup(node);
                }

                return baseNode;
            }
        }

        private static GameObject baseNode;
        private static GameObject baseInput;
    }
}
