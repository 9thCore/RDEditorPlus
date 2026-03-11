using RDEditorPlus.Functionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Variable;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes
{
    public class Node : MonoBehaviour, INodeWorkspace.INode, ISerializableNodeWorkspace.INode
    {
        public static GameObject PreparePrefab(string name, NodeVariable.Data[] variables, NodeInput.Data[] inputs, NodeOutput.Data[] outputs)
        {
            GameObject root = Instantiate(BaseNode);
            root.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_Node{name}";

            var node = root.GetComponent<Node>();
            node.SetName(name);
            node.variables = new();
            node.inputs = new();
            node.outputs = new();

            foreach (var variable in variables)
            {
                variable.Setup(node);
            }

            foreach (var input in inputs)
            {
                input.Setup(node);
            }

            foreach (var output in outputs)
            {
                output.Setup(node);
            }

            node.ScheduleUIUpdate();
            return root;
        }

        public void Setup(NodeGrid grid)
        {
            this.grid = grid;
        }

        public void GenerateID(string id)
        {
            this.id = id.IsNullOrEmpty() ? LevelEvent_MakeSprite.RandomString(8) : id;
        }

        public NodeInput GetInput(string name)
        {
            return inputs.Where(input => input.Name == name).FirstOrDefault();
        }

        public NodeOutput GetOutput(string name)
        {
            return outputs.Where(output => output.Name == name).FirstOrDefault();
        }

        public NodeConnection CreateConnection() => grid.CreateConnection();
        public NodeConnection VirtualConnection => grid.VirtualConnection;

        public void AddVariable(RectTransform transform, NodeVariable variable)
        {
            transform.SetParent(variableParent);
            variables.Add(variable);
        }

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

            if (variables.Any(variable => variable.CanSave()))
            {
                await writer.WriteStartElementAsync(VariablesKey);
                foreach (var variable in variables.Where(variable => variable.CanSave()))
                {
                    await writer.WriteStartElementAsync(VariableKey);
                    await variable.SaveAsync(writer);
                    await writer.WriteEndElementAsync();
                }
                await writer.WriteEndElementAsync();
            }

            if (inputs.Any(input => input.CanSave()))
            {
                await writer.WriteStartElementAsync(InputsKey);
                foreach (var input in inputs.Where(input => input.CanSave()))
                {
                    await writer.WriteStartElementAsync(InputKey);
                    await input.SaveAsync(writer);
                    await writer.WriteEndElementAsync();
                }
                await writer.WriteEndElementAsync();
            }
        }

        public void SetAllNodesAsAccessible() => grid.SetAllNodesAsAccessible();

        public void PropagateInaccessibilityThroughInputs()
        {
            Accessible = false;

            foreach (var input in inputs)
            {
                input.PropagateInaccessibility();
            }
        }

        public void PropagateInaccessibilityThroughOutputs()
        {
            Accessible = false;

            foreach (var output in outputs)
            {
                output.PropagateInaccessibility();
            }
        }

        public void PropagateDependenciesSaved()
        {
            foreach (var output in outputs)
            {
                output.PropagateDependenciesSaved();
            }
        }

        public void PostDeserialise()
        {
            
        }

        public void SetVariable(string name, object value)
        {
            var variable = variables.Where(variable => variable.Name == name).FirstOrDefault();
            if (variable == null)
            {
                return;
            }

            variable.SetValue(value.ToString());
        }

        public bool InputDependenciesSaved
        {
            get => inputs.All(input => input.DependenciesSaved);
            set
            {
                foreach (var input in inputs)
                {
                    input.DependenciesSaved = value;
                }
            }
        }

        public string Id => id;

        public bool Accessible
        {
            get => accessible;
            set => accessible = value;
        }

        public bool AlreadySaved
        {
            get => alreadySaved;
            set => alreadySaved = value;
        }

        public ISerializableNodeWorkspace.INode.IInput[] Inputs => inputs.ToArray();
        public ISerializableNodeWorkspace.INode.IVariable[] Variables => variables.ToArray();
        public string Name => nodeName;
        public Vector2 Position => rectTransform.anchoredPosition;

        public enum Type
        {
            Float,
            Integer,
            String
        }

        private void SetName(string name)
        {
            nodeName = name;
            title.text = name;
        }
        
        // This should only be run on the prefab to ensure the UI is setup properly
        private void ScheduleUIUpdate()
        {
            gameObject.SetActive(true);
            StartCoroutine(DeactivateImmediatelyAterUIUpdate());
        }

        private IEnumerator DeactivateImmediatelyAterUIUpdate()
        {
            // idk why it has to wait twice
            yield return null;
            yield return null;
            gameObject.SetActive(false);
        }

        [SerializeField]
        private NodeGrid grid;

        [SerializeField]
        private Text title;

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private RectTransform variableParent;

        [SerializeField]
        private RectTransform inputParent;

        [SerializeField]
        private RectTransform outputParent;

        [SerializeField]
        private List<NodeVariable> variables;

        [SerializeField]
        private List<NodeInput> inputs;

        [SerializeField]
        private List<NodeOutput> outputs;

        [SerializeField]
        private string nodeName;

        private string id;
        private bool accessible;
        private bool alreadySaved;

        public static Font Font;
        public static Sprite Sprite;

        public const int TopPadding = 4;
        public const int BottomPadding = 4;
        public const float RootSpacing = 4f;
        public const float InterfaceSpacing = 8f;
        public const float ConnectorSpacing = 6f;

        public const string IDKey = "id";
        public const string NameKey = "name";
        public const string PositionKey = "Position";
        public const string PositionXKey = "x";
        public const string PositionYKey = "y";
        public const string VariablesKey = "Variables";
        public const string VariableKey = "Variable";
        public const string InputsKey = "Inputs";
        public const string InputKey = "Input";

        private static GameObject BaseNode
        {
            get
            {
                if (baseNode == null)
                {
                    baseNode = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_NodeBase");
                    baseNode.SetActive(false);

                    var node = baseNode.AddComponent<Node>();
                    baseNode.AddComponent<NodeEventTrigger>().Setup(node);

                    baseNode.AddComponent<FourSidedOutline>().effectColor = Color.black;
                    baseNode.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);

                    baseNode.AddComponent<VerticalLayoutGroup>();
                    baseNode.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var background = baseNode.AddComponent<Image>();
                    background.sprite = Sprite;
                    background.color = Color.gray.WithBrightness(0.8f);
                    background.type = Image.Type.Tiled;

                    var transform = node.transform as RectTransform;

                    node.rectTransform = transform;

                    #region Node layout root
                    GameObject root = new("layoutRoot");
                    root.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var layoutRoot = root.AddComponent<VerticalLayoutGroup>();
                    layoutRoot.childForceExpandWidth = true;
                    layoutRoot.childForceExpandHeight = false;
                    layoutRoot.childControlWidth = true;
                    layoutRoot.childControlHeight = false;
                    layoutRoot.padding = new(0, 0, TopPadding, BottomPadding);
                    layoutRoot.spacing = RootSpacing;

                    var layoutRT = root.transform as RectTransform;
                    layoutRT.SetParent(transform);
                    layoutRT.pivot = new Vector2(0.5f, 1f);
                    layoutRT.anchorMin = Vector2.zero;
                    layoutRT.anchorMax = Vector2.one;
                    layoutRT.offsetMin = layoutRT.offsetMax = Vector2.zero;
                    #endregion

                    #region Node title
                    GameObject title = new("title");
                    title.AddComponent<EightSidedOutline>().effectColor = Color.black;
                    title.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    title.transform.SetParent(baseNode.transform);
                    title.transform.localPosition = Vector3.zero;
                    title.transform.localScale = Vector3.one;

                    node.title = title.AddComponent<Text>();
                    node.title.font = Font;
                    node.title.alignment = TextAnchor.UpperCenter;
                    node.title.fontSize = 8;

                    var titleRT = node.title.rectTransform;
                    titleRT.SetParent(layoutRT);
                    titleRT.anchorMin = new Vector2(0f, 1f);
                    titleRT.anchorMax = Vector2.one;
                    titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;
                    #endregion

                    #region Node variables
                    GameObject variables = new("variables");
                    variables.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var variableGroup = variables.AddComponent<VerticalLayoutGroup>();
                    variableGroup.childAlignment = TextAnchor.UpperLeft;
                    variableGroup.childForceExpandWidth = true;
                    variableGroup.childForceExpandHeight = false;
                    variableGroup.childControlWidth = true;
                    variableGroup.childControlHeight = true;
                    variableGroup.spacing = ConnectorSpacing;

                    var variablesRT = variables.transform as RectTransform;
                    variablesRT.SetParent(layoutRT);
                    variablesRT.anchorMin = Vector2.zero;
                    variablesRT.anchorMax = Vector2.one;
                    variablesRT.offsetMin = variablesRT.offsetMax = Vector2.zero;
                    variablesRT.pivot = new Vector2(0.5f, 1f);

                    node.variableParent = variablesRT;
                    #endregion

                    #region Node connectors
                    GameObject connectors = new("connectors");
                    connectors.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var connectorGroup = connectors.AddComponent<HorizontalLayoutGroup>();
                    connectorGroup.childAlignment = TextAnchor.UpperCenter;
                    connectorGroup.childForceExpandWidth = true;
                    connectorGroup.childForceExpandHeight = false;
                    connectorGroup.childControlWidth = true;
                    connectorGroup.childControlHeight = false;
                    connectorGroup.spacing = InterfaceSpacing;

                    var connectorsRT = connectors.transform as RectTransform;
                    connectorsRT.SetParent(layoutRT);
                    connectorsRT.anchorMin = Vector2.zero;
                    connectorsRT.anchorMax = Vector2.one;
                    connectorsRT.offsetMin = connectorsRT.offsetMax = Vector2.zero;
                    #endregion

                    #region Node inputs
                    GameObject inputs = new("inputs");
                    inputs.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var inputGroup = inputs.AddComponent<VerticalLayoutGroup>();
                    inputGroup.childAlignment = TextAnchor.UpperLeft;
                    inputGroup.childForceExpandWidth = true;
                    inputGroup.childForceExpandHeight = false;
                    inputGroup.childControlWidth = true;
                    inputGroup.childControlHeight = true;
                    inputGroup.spacing = ConnectorSpacing;

                    var inputsRT = inputs.transform as RectTransform;
                    inputsRT.SetParent(connectorsRT);
                    inputsRT.offsetMin = inputsRT.offsetMax = Vector2.zero;
                    inputsRT.pivot = new Vector2(0.5f, 1f);

                    node.inputParent = inputsRT;
                    #endregion

                    #region Node outputs
                    GameObject outputs = new("outputs");
                    outputs.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var outputGroup = outputs.AddComponent<VerticalLayoutGroup>();
                    outputGroup.childAlignment = TextAnchor.UpperRight;
                    outputGroup.childForceExpandWidth = true;
                    outputGroup.childForceExpandHeight = false;
                    outputGroup.childControlWidth = true;
                    outputGroup.childControlHeight = true;
                    outputGroup.spacing = ConnectorSpacing;

                    var outputsRT = outputs.transform as RectTransform;
                    outputsRT.SetParent(connectorsRT);
                    outputsRT.offsetMin = outputsRT.offsetMax = Vector2.zero;
                    outputsRT.pivot = new Vector2(0.5f, 1f);

                    node.outputParent = outputsRT;
                    #endregion
                }

                return baseNode;
            }
        }

        private static GameObject baseNode;
    }
}
