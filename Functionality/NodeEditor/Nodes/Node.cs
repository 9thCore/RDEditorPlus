using RDEditorPlus.Functionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

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
            node.inputs = new();
            node.outputs = new();

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

        public void Setup(NodeGrid grid)
        {
            this.grid = grid;
        }

        public void GenerateID()
        {
            id = LevelEvent_MakeSprite.RandomString(8);
        }

        public NodeConnection CreateConnection() => grid.CreateConnection();
        public NodeConnection VirtualConnection => grid.VirtualConnection;

        public void AddInput(RectTransform transform, NodeInput input)
        {
            transform.SetParent(inputParent);
            inputs.Add(input);
        }

        public void AddOutput(RectTransform transform, NodeOutput output)
        {
            transform.SetParent(outputParent);
            outputs.Add(output);
        }

        public void Drag(Vector2 delta)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            Vector2 scaledDelta = delta / rectTransform.lossyScale;
            rectTransform.anchoredPosition += scaledDelta;

            foreach (var input in inputs)
            {
                input.Drag(delta);
            }

            foreach (var output in outputs)
            {
                output.Drag();
            }
        }

        public void Delete(bool dontDeleteFromGrid)
        {
            foreach (var input in inputs)
            {
                input.Unlink();
            }

            foreach (var output in outputs)
            {
                output.Unlink();
            }

            if (!dontDeleteFromGrid)
            {
                grid.DeleteNode(this);
            }

            gameObject.SetActive(false);
            GameObject.Destroy(gameObject);
        }

        public async Task SaveAsync(XmlWriter writer)
        {
            await writer.WriteAttributeStringAsync(IDKey, id);
            await writer.WriteAttributeStringAsync(NameKey, nodeName);

            Vector2 position = rectTransform.anchoredPosition;

            await writer.WriteStartElementAsync(PositionKey);
            await writer.WriteAttributeStringAsync(PositionXKey, position.x.ToString());
            await writer.WriteAttributeStringAsync(PositionYKey, position.y.ToString());
            await writer.WriteEndElementAsync();

            await writer.WriteStartElementAsync(OutputsKey);
            foreach (var output in outputs)
            {
                if (output.CanSave())
                {
                    await writer.WriteStartElementAsync(OutputKey);
                    await output.SaveAsync(writer);
                    await writer.WriteEndElementAsync();
                }
            }
            await writer.WriteEndElementAsync();
        }

        public enum Type
        {
            Float
        }

        private void SetName(string name)
        {
            nodeName = name;
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

        public string Id => id;

        [SerializeField]
        private NodeGrid grid;

        [SerializeField]
        private Text title;

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private RectTransform inputParent;

        [SerializeField]
        private RectTransform outputParent;

        [SerializeField]
        private List<NodeInput> inputs;

        [SerializeField]
        private List<NodeOutput> outputs;

        [SerializeField]
        private string nodeName;

        private string id;

        public static Font Font;
        public static Sprite Sprite;

        public const float TextHeight = 18f;
        public const float TextClearance = 4f;
        public const float InterfaceSpacing = 8f;
        public const float ConnectorSpacing = 6f;

        public const string IDKey = "id";
        public const string NameKey = "name";
        public const string PositionKey = "Position";
        public const string PositionXKey = "x";
        public const string PositionYKey = "y";
        public const string OutputsKey = "Outputs";
        public const string OutputKey = "Output";

        private static GameObject BaseNode
        {
            get
            {
                if (baseNode == null)
                {
                    baseNode = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_NodeBase");
                    baseNode.SetActive(false);

                    baseNode.AddComponent<FourSidedOutline>().effectColor = Color.black;
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
