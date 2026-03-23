using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Attributes;
using RDEditorPlus.Util;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector
{
    public abstract class NodeConnector : MonoBehaviour
    {
        public string Id => node.Id;
        public string Name => connectorName;

        public abstract void StartConnection();
        public abstract void UpdateConnection(Vector2 pointerPosition);
        public abstract void EndConnection(NodeConnector selectedNode);
        public abstract void SetColor(Node.Type nodeType, Type type);
        public abstract void ResetColor();
        public abstract void Unlink(bool justReplace);
        public abstract void PropagateInaccessibility();
        public abstract void ResetColorOverrideType();
        public abstract void SetColorOverrideType(Node.Type type);
        public abstract bool ConnectedTo(NodeConnector connector);
        public abstract bool ConnectedToAnything { get; }

        public enum Type
        {
            Input = 0,
            Output = 1
        }

        public record Link(NodeInput Input, NodeOutput Output, NodeConnection Connection)
        {
            public void Unlink(bool dontRaiseDisconnectEvent)
            {
                Connection.Delete();
                Output.Remove(this);
                Input.RemoveLink();

                Input.ResetColorOverrideType();

                if (!dontRaiseDisconnectEvent)
                {
                    Input.node.RaiseDisconnectEvent();
                    Output.node.RaiseDisconnectEvent();
                }
            }

            public async Task SaveAsync(XmlWriter writer)
            {
                await writer.WriteAttributeStringAsync(TargetKey, Output.Id);
                await writer.WriteAttributeStringAsync(OutputKey, Output.connectorName);
            }

            public const string TargetKey = "target";
            public const string OutputKey = "output";
        }

        public const string NameKey = "name";

        protected internal abstract void SetupConnection(NodeConnector other, bool justReplace);

        protected static void SetColorToAll(Node.Type nodeType, Type type)
        {
            foreach (var connector in allConnectors)
            {
                connector.SetColor(nodeType, type);
            }
        }

        protected static void ResetColorToAll()
        {
            foreach (var connector in allConnectors)
            {
                connector.ResetColor();
            }
        }

        [SerializeField]
        protected internal Node node;
        [SerializeField]
        protected string connectorName;

        [SerializeField]
        protected internal RectTransform control;
        protected internal bool available = false;

        protected static readonly HashSet<NodeConnector> allConnectors = new();

        protected static Type GetOppositeType(Type type)
        {
            return 1 - type;
        }
    }

    public abstract class NodeConnector<ConnectorType, PrefabProvider>(NodeConnector.Type connectorType) : NodeConnector
        where ConnectorType : NodeConnector<ConnectorType, PrefabProvider>
        where PrefabProvider : IPrefabProvider, new()
    {
        public override void StartConnection()
        {
            node.VirtualConnection.SetStartColor(ValidControlColor);
            node.VirtualConnection.SetAnchor(control);
            node.VirtualConnection.gameObject.SetActive(true);

            node.SetAllNodesAsAccessible();
            if (connectorType == Type.Input)
            {
                node.PropagateInaccessibilityThroughOutputs();
            }
            else
            {
                node.PropagateInaccessibilityThroughInputs();
            }

            SetColorToAll(colorOverrideType ?? type, GetOppositeType(connectorType));
            ResetColor();
        }

        public override void UpdateConnection(Vector2 pointerPosition)
        {
            node.VirtualConnection.SetEndPoint(pointerPosition);
        }

        public override void EndConnection(NodeConnector selectedNode)
        {
            node.VirtualConnection.gameObject.SetActive(false);

            if (selectedNode != null && selectedNode != this
                && selectedNode.available && !ConnectedTo(selectedNode))
            {
                NodeConnector input = connectorType == Type.Input ? this : selectedNode;
                NodeConnector output = connectorType == Type.Output ? this : selectedNode;

                output.SetupConnection(input, justReplace: input.ConnectedToAnything);
            }

            ResetColorToAll();
        }

        public override void SetColor(Node.Type nodeType, Type type)
        {
            bool compatibile = connectorType == Type.Input
                ? Node.AreCompatible(nodeType, this.type)
                : Node.AreCompatible(this.type, nodeType);

            if (compatibile && node.Accessible && connectorType == type)
            {
                controlImage.color = GetColor.ValidControl;
                controlOutline.effectColor = OutlineSelectableColor;
                available = true;
            }
            else
            {
                controlImage.color = GetColor.InvalidControl;
                controlOutline.effectColor = OutlineUnselectableColor;
                available = false;
            }
        }

        public override void ResetColor()
        {
            controlImage.color = GetColor.ValidControl;
            controlOutline.effectColor = OutlineUnselectableColor;
        }

        public override void ResetColorOverrideType()
        {
            colorOverrideType = null;
            ResetColor();
        }

        public override void SetColorOverrideType(Node.Type type)
        {
            colorOverrideType = type;
            ResetColor();
        }

        public void ResetUsageOverrideType()
        {
            usageOverrideType = null;
        }

        public void SetUsageOverrideType(Node.Type type)
        {
            usageOverrideType = type;
        }

        public Node.Type? ColorOverrideType => colorOverrideType;

        protected abstract void AddToNode(Node node, string description);
        protected abstract void PrefabSetup();

        protected Color ValidControlColor => GetColor.ValidControl;
        protected void SetName(string text)
        {
            // epic line
            this.text.text = text;
            connectorName = text;
        }

        protected void OnEnable()
        {
            allConnectors.Add(this);
        }

        protected void OnDisable()
        {
            allConnectors.Remove(this);
        }

        private void SetType(Node.Type type)
        {
            this.type = type;
            ResetColor();
        }

        private ConnectorColorAttribute GetColor => Node.GetColor(colorOverrideType ?? type);

        [SerializeField]
        protected RectTransform rectTransform;
        [SerializeField]
        protected Text text;
        [SerializeField]
        protected Node.Type type;
        [SerializeField]
        protected Node.Type? colorOverrideType;
        [SerializeField]
        protected Node.Type? usageOverrideType;
        [SerializeField]
        protected Image controlImage;
        [SerializeField]
        protected EightSidedOutline controlOutline;

        protected readonly Type connectorType = connectorType;

        public readonly struct Data(Node.Type type, string name, string description)
        {
            public readonly void Setup(Node node)
            {
                var connector = Get(type, name);
                if (connector != null)
                {
                    connector.node = node;
                    connector.AddToNode(node, description);
                }
            }
        }

        protected static GameObject BaseConnector(string connectorType)
        {
            GameObject baseConnector = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_Node{connectorType}");
            baseConnector.SetActive(false);

            var rt = baseConnector.AddComponent<RectTransform>();

            GameObject control = new("control");

            var outline = control.AddComponent<EightSidedOutline>();
            outline.effectColor = Color.gray;

            var image = control.AddComponent<Image>();
            image.sprite = AssetUtil.InputFieldSprite;
            image.type = Image.Type.Tiled;

            var controlRT = control.transform as RectTransform;
            controlRT.SetParent(rt);
            controlRT.offsetMin = new Vector2(-3f, -3f);
            controlRT.offsetMax = -controlRT.offsetMin;

            var connector = baseConnector.AddComponent<ConnectorType>();
            connector.rectTransform = rt;
            connector.control = controlRT;
            connector.controlImage = image;

            GameObject text = new("name");

            connector.text = text.AddComponent<Text>();
            connector.text.ApplyRDFont();
            connector.text.alignment = TextAnchor.MiddleLeft;
            connector.text.verticalOverflow = VerticalWrapMode.Overflow;

            text.AddComponent<EightSidedOutline>().effectColor = Color.black;

            var titleRT = connector.text.rectTransform;
            titleRT.SetParent(rt);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

            baseConnector.AddComponent<TextLayoutElementPropagator>().Setup(connector.text);

            connector.PrefabSetup();
            control.AddComponent<NodeConnectorControlEventTrigger>().Setup(connector);

            connector.controlOutline = outline;
            return baseConnector;
        }

        protected static GameObject SetupConnector(GameObject baseConnector, Node.Type type)
        {
            GameObject connector = Instantiate(baseConnector);

            connector.name = $"{baseConnector.name}{type}";
            connector.GetComponent<ConnectorType>().SetType(type);
            return connector;
        }

        private static NodeConnector<ConnectorType, PrefabProvider> Get(Node.Type type, string name)
        {
            var instance = Instantiate(GetPrefab(type));
            var connector = instance.GetComponent<ConnectorType>();
            connector.SetName(name);

            instance.SetActive(true);
            return connector;
        }

        private readonly Color OutlineUnselectableColor = Color.gray;
        private readonly Color OutlineSelectableColor = Color.yellow;

        private static GameObject GetPrefab(Node.Type type)
        {
            if (prefabCache.TryGetValue(type, out var prefab)
                && prefab != null)
            {
                return prefab;
            }

            prefab = SetupConnector(BaseConnectorPrefab, type);
            prefabCache[type] = prefab;
            return prefab;
        }

        private static GameObject BaseConnectorPrefab
        {
            get
            {
                if (baseConnector == null)
                {
                    baseConnector = BaseConnector(new PrefabProvider().ConnectorType.ToString());
                }

                return baseConnector;
            }
        }

        private static readonly Dictionary<Node.Type, GameObject> prefabCache = new();

        private static GameObject baseConnector;
    }
}
