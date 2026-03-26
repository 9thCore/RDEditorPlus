using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes.Modifier;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Attributes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable.Enums;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable.RDLevelNode;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes
{
    public class Node : MonoBehaviour, INodeWorkspace.INode, ISerializableNodeWorkspace.INode
    {
        public static GameObject PreparePrefab(
            string name,
            NodeVariable.Data[] variables,
            NodeInput.Data[] inputs,
            NodeOutput.Data[] outputs,
            NodeModifierAttribute attribute)
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

            attribute?.Apply(root, node);

            node.ScheduleUIUpdate();
            return root;
        }

        public static ConnectorColorAttribute GetColor(Type type) => information[type].Color;
        public static VariableTypeAttribute GetVariableType(Type type) => information[type].VariableType;
        public static bool CanConvertToMath(Type type) => information[type].Convertible?.CanDoMath ?? false;

        public static bool AreCompatible(Type outputType, Type inputType)
        {
            if (inputType == Type.MathConvertible)
            {
                return CanConvertToMath(outputType);
            }
            else if (outputType == Type.MathConvertible)
            {
                return CanConvertToMath(inputType);
            }

            return inputType == outputType;
        }

        public static Type GetBestFitFor(Type a, Type b)
        {
            if (a == Type.FloatExpression2 || b == Type.FloatExpression2)
            {
                return Type.FloatExpression2;
            }
            else if ((a == Type.FloatExpression && b == Type.Float2)
                || (a == Type.Float2 && b == Type.FloatExpression))
            {
                return Type.FloatExpression2;
            }

            return (Type)Math.Max((int)a, (int)b);
        }

        public void Setup(NodeGrid grid)
        {
            this.grid = grid;
        }

        public string GenerateID(string id)
        {
            this.id = id.IsNullOrEmpty() ? LevelEvent_MakeSprite.RandomString(8) : id;
            return this.id;
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

        public void AddVariable(RectTransform transform, NodeVariable variable, string description)
        {
            transform.gameObject.AddComponent<NodeDescriptionEventTrigger>().Setup(description);
            transform.SetParent(variableParent);
            variables.Add(variable);
        }

        public void AddInput(RectTransform transform, NodeInput input, string description)
        {
            transform.gameObject.AddComponent<NodeDescriptionEventTrigger>().Setup(description);
            transform.SetParent(inputParent);
            inputs.Add(input);
        }

        public void AddOutput(RectTransform transform, NodeOutput output, string description)
        {
            transform.gameObject.AddComponent<NodeDescriptionEventTrigger>().Setup(description);
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
                input.Drag();
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
                input.Unlink(dontRaiseDisconnectEvent: false);
            }

            foreach (var output in outputs)
            {
                output.Unlink(dontRaiseDisconnectEvent: false);
            }

            if (!dontDeleteFromGrid)
            {
                grid.DeleteNode(this);
            }

            gameObject.SetActive(false);
            Destroy(gameObject);
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

        public void SubscribeToConnect(Action action)
        {
            onConnect += action;
        }

        public void SubscribeToDisconnect(Action action)
        {
            onDisconnect += action;
        }

        public void SubscribeToReplace(Action action)
        {
            onReplace += action;
        }

        public void RaiseConnectEvent()
        {
            onConnect?.Invoke();
        }

        public void RaiseDisconnectEvent()
        {
            onDisconnect?.Invoke();
        }

        public void RaiseReplaceEvent()
        {
            onReplace?.Invoke();
        }

        public NodeGrid.NodeTarget[] InputTargets => inputs.Select(input => input.NodeTarget).ToArray();
        public NodeGrid.NodeTarget[][] OutputTargets => outputs.Select(output => output.Targets).ToArray();
        public string[] SerialisedVariables => variables.Select(variable => variable.Value.ToString()).ToArray();

        public void ConnectInputs(NodeGrid.NodeTarget[] targets)
        {
            int index = 0;
            foreach (var target in targets)
            {
                SetInput(index, target);
                index++;
            }
        }

        public void ConnectOutputs(NodeGrid.NodeTarget[][] outputTargets)
        {
            int index = 0;
            foreach (var targets in outputTargets)
            {
                SetOutput(index, targets);
                index++;
            }
        }

        public void SetupVariables(string[] variableValues)
        {
            int index = 0;
            foreach (var variableValue in variableValues)
            {
                if (variableValue != null)
                {
                    variables[index].SetValue(variableValue);
                }

                index++;
            }
        }

        public void SetInput(int index, NodeGrid.NodeTarget target)
        {
            if (target.ID != null && grid.TryGetNodeFromID(target.ID, out var output))
            {
                output.GetOutput(target.Name).SetupConnection(inputs[index], justReplace: false);
            }
        }

        public void SetOutput(int index, NodeGrid.NodeTarget[] targets)
        {
            if (targets == null)
            {
                return;
            }

            var output = outputs[index];

            foreach (var target in targets)
            {
                if (grid.TryGetNodeFromID(target.ID, out var input))
                {
                    output.SetupConnection(input.GetInput(target.Name), justReplace: false);
                }
            }
        }

        public void ClearInput(int index) => inputs[index].Unlink(dontRaiseDisconnectEvent: false);
        public void ClearOutput(int index) => outputs[index].Unlink(dontRaiseDisconnectEvent: false);

        public void SendDragEvent(Vector3 oldPosition)
            => grid.RegisterMoveNodeAction(this, oldPosition, transform.position);

        public void SendLinkEvent(Node input, string inputName, string outputName)
            => grid.RegisterLinkNodesAction(input, this, inputName, outputName);

        public void SendReplaceLinkEvent(Node input, Node replace, string inputName, string outputName, string replaceName)
            => grid.RegisterReplaceLinkNodesAction(input, this, replace, inputName, outputName, replaceName);

        public void SendDeleteEvent() => grid.RegisterDeleteNodeAction(this, rectTransform.anchoredPosition);

        public void SendVariableChangeEvent(string name, string oldValue, string newValue)
            => grid.RegisterVariableChangeNodeAction(this, name, oldValue, newValue);

        public void SendInputUnlinkEvent(NodeInput input, NodeGrid.NodeTarget target)
            => grid.RegisterNodeInputRemoveLinkAction(this, inputs.IndexOf(input), target);

        public void SendOutputUnlinkEvent(NodeOutput output, NodeGrid.NodeTarget[] targets)
            => grid.RegisterNodeOutputRemoveLinkAction(this, outputs.IndexOf(output), targets);

        public enum Type
        {
            [ConnectorColor("0000FF"), VariableType<IntegerNodeVariable>,
                Convertible(TypeConvertible.Math)] Int32,
            [ConnectorColor("00FF00"), VariableType<FloatNodeVariable>,
                Convertible(TypeConvertible.Math)] Single,
            [ConnectorColor("8080FF"), VariableType<FloatExpressionNodeVariable>,
                Convertible(TypeConvertible.Math)] FloatExpression,
            [ConnectorColor("00FF80"), VariableType<Float2NodeVariable>,
                Convertible(TypeConvertible.Math)] Float2,
            [ConnectorColor("FF80FF"), VariableType<FloatExpression2NodeVariable>,
                Convertible(TypeConvertible.Math)] FloatExpression2,

            [ConnectorColor("FFFF00"), VariableType<StringNodeVariable>] String,
            [ConnectorColor("FF0000"), VariableType<BooleanNodeVariable>] Boolean,

            [ConnectorColor("FF00FF"), VariableType<MathOperationNodeVariable>] MathOperation,

            /// <summary>
            /// Node connector that can connect with anything that has the Convertible attribute set with TypeConvertible.Math
            /// </summary>
            [ConnectorColor("FFFFFF")] MathConvertible,

            [VariableType<RDLevelNodeVariable>] RDLevelFile,
            [VariableType<RDLevelSaveNodeVariable>] RDLevelSaveFile,
            [ConnectorColor("FF0000")] RDLevelSettings,
            [ConnectorColor("0080FF")] RDLevelRows,
            [ConnectorColor("00FF00")] RDLevelSprites,
            [ConnectorColor("FF00FF")] RDLevelEvents,
            [ConnectorColor("FF0080")] RDLevelConditionals,
            [ConnectorColor("FFFFFF")] RDLevelBookmarks,
            [ConnectorColor("FF80FF")] RDLevelPalette,
            [ConnectorColor("00FFFF")] RDLevelAssets
        }

        [Flags]
        public enum TypeConvertible
        {
            Math = 1 << 0
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
            // idk why it has to wait thrice
            yield return null;
            yield return null;
            yield return null;

            gameObject.SetActive(false);

            yield return null;

            foreach (var component in autoLayoutGroups)
            {
                GameObject.DestroyImmediate(component);
            }

            foreach (var component in autoLayoutFitters)
            {
                GameObject.DestroyImmediate(component);
            }

            autoLayoutGroups = null;
            autoLayoutFitters = null;
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

        [SerializeField]
        private HorizontalOrVerticalLayoutGroup[] autoLayoutGroups;

        [SerializeField]
        private ContentSizeFitter[] autoLayoutFitters;

        private Action onConnect, onDisconnect, onReplace;
        private string id;
        private bool accessible;
        private bool alreadySaved;

        public const int TopPadding = 4;
        public const int BottomPadding = 4;
        public const float RootSpacing = 4f;
        public const float InterfaceSpacing = 2f;
        public const float ConnectorSpacing = 4f;

        public const string IDKey = "id";
        public const string NameKey = "name";
        public const string PositionKey = "Position";
        public const string PositionXKey = "x";
        public const string PositionYKey = "y";
        public const string VariablesKey = "Variables";
        public const string VariableKey = "Variable";
        public const string InputsKey = "Inputs";
        public const string InputKey = "Input";

        static Node()
        {
            foreach(var value in Enum.GetValues(typeof(Type)).Cast<Type>())
            {
                information.Add(
                    value,
                    new(
                        Color: value.GetAttributeOfType<ConnectorColorAttribute>(),
                        VariableType: value.GetAttributeOfType<VariableTypeAttribute>(),
                        Convertible: value.GetAttributeOfType<ConvertibleAttribute>())
                    );
            }
        }

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
                    background.sprite = AssetUtil.ButtonSprite;
                    background.color = Color.gray.WithBrightness(0.8f);
                    background.type = Image.Type.Tiled;

                    var transform = node.transform as RectTransform;
                    transform.SizeDeltaX(120f);

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
                    node.title.ApplyRDFont();
                    node.title.alignment = TextAnchor.UpperCenter;

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

                    node.autoLayoutGroups = baseNode.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(includeInactive: true);
                    node.autoLayoutFitters = baseNode.GetComponentsInChildren<ContentSizeFitter>(includeInactive: true);
                }

                return baseNode;
            }
        }

        private static GameObject baseNode;
        private static readonly Dictionary<Type, TypeInformation> information = new();

        private record struct TypeInformation(ConnectorColorAttribute Color, VariableTypeAttribute VariableType, ConvertibleAttribute Convertible);
    }
}
