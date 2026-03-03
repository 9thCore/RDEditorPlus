using DG.Tweening;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Audio.Handle;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes
{
    public class Node : MonoBehaviour
    {
        public static GameObject PreparePrefab(string name, IEnumerable<NodeInput.Data> inputs, IEnumerable<NodeOutput.Data> outputs)
        {
            GameObject root = Instantiate(BaseNode);
            root.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_Node{name}";

            var node = root.GetComponent<Node>();
            node.SetName(name);

            foreach (var input in inputs)
            {
                input.Setup(node);
            }

            foreach (var output in outputs)
            {
                output.Setup(node);
            }

            return root;
        }

        public void AddInput(RectTransform input) => input.SetParent(inputParent);
        public void AddOutput(RectTransform output) => output.SetParent(outputParent);

        public void Drag(Vector2 delta)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            rectTransform.anchoredPosition += delta / rectTransform.lossyScale;
        }

        public enum Type
        {
            Float
        }

        private void SetName(string name)
        {
            title.text = name;
        }

        private void Start()
        {
            StartCoroutine(SetupVisualsProperly());
        }

        private IEnumerator SetupVisualsProperly()
        {
            yield return new WaitForEndOfFrame();

            rectTransform.offsetMin -= Vector2.up * (Mathf.Max(inputParent.rect.height, outputParent.rect.height) + TextClearance + TextHeight);
        }

        [SerializeField]
        private Text title;

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private RectTransform inputParent;

        [SerializeField]
        private RectTransform outputParent;

        public static Font Font;
        public static Sprite Sprite;

        public const float TextHeight = 18f;
        public const float TextClearance = 4f;
        public const float InterfaceSpacing = 8f;
        public const float ConnectorSpacing = 6f;

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

                    GameObject controls = new("controls");
                    var group = controls.AddComponent<HorizontalLayoutGroup>();
                    group.childAlignment = TextAnchor.UpperCenter;
                    group.childForceExpandHeight = false;
                    group.childControlHeight = false;
                    group.spacing = InterfaceSpacing;

                    var controlsRT = controls.transform as RectTransform;
                    controlsRT.SetParent(baseNode.transform);
                    controlsRT.offsetMin = Vector2.zero;
                    controlsRT.offsetMax = new Vector2(0f, -(TextHeight + TextClearance));
                    controlsRT.anchorMin = Vector2.zero;
                    controlsRT.anchorMax = Vector2.one;

                    GameObject inputs = new("inputs");
                    var inputGroup = inputs.AddComponent<VerticalLayoutGroup>();
                    inputGroup.childAlignment = TextAnchor.UpperLeft;
                    inputGroup.childForceExpandHeight = false;
                    inputGroup.spacing = ConnectorSpacing;

                    inputs.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var inputsRT = inputs.transform as RectTransform;
                    inputsRT.SetParent(controlsRT);
                    inputsRT.offsetMin = inputsRT.offsetMax = Vector2.zero;
                    inputsRT.pivot = new Vector2(0.5f, 1f);

                    GameObject outputs = new("outputs");
                    var outputGroup = outputs.AddComponent<VerticalLayoutGroup>();
                    outputGroup.childAlignment = TextAnchor.UpperRight;
                    outputGroup.childForceExpandHeight = false;
                    outputGroup.spacing = ConnectorSpacing;

                    outputs.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var outputsRT = outputs.transform as RectTransform;
                    outputsRT.SetParent(controlsRT);
                    outputsRT.offsetMin = outputsRT.offsetMax = Vector2.zero;
                    outputsRT.pivot = new Vector2(0.5f, 1f);

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
                    titleRT.anchorMin = new Vector2(0f, 1f);
                    titleRT.anchorMax = Vector2.one;
                    titleRT.offsetMin = new Vector2(0f, -TextHeight);
                    titleRT.offsetMax = Vector2.zero;

                    var rt = baseNode.transform as RectTransform;
                    rt.offsetMin = new Vector2(-50f, 0f);
                    rt.offsetMax = -rt.offsetMin;

                    node.rectTransform = rt;
                    node.inputParent = inputsRT;
                    node.outputParent = outputsRT;
                    baseNode.AddComponent<NodeEventTrigger>().Setup(node);
                }

                return baseNode;
            }
        }

        private static GameObject baseNode;
    }
}
