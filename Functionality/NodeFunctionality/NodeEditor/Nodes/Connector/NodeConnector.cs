using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
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
        public abstract void Unlink();
        public abstract void PropagateInaccessibility();

        public enum Type
        {
            Input = 0,
            Output = 1
        }

        public record Link(NodeInput Input, NodeOutput Output, NodeConnection Connection)
        {
            public void Unlink()
            {
                Connection.Delete();
                Output.Remove(this);
                Input.RemoveLink();
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

        protected internal abstract void SetupConnection(NodeConnector other);

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

            SetColorToAll(type, GetOppositeType(connectorType));
            ResetColor();
        }

        public override void UpdateConnection(Vector2 pointerPosition)
        {
            node.VirtualConnection.SetEndPoint(pointerPosition);
        }

        public override void EndConnection(NodeConnector selectedNode)
        {
            node.VirtualConnection.gameObject.SetActive(false);

            if (selectedNode != null && selectedNode != this && selectedNode.available)
            {
                SetupConnection(selectedNode);
            }

            ResetColorToAll();
        }

        public override void SetColor(Node.Type nodeType, Type type)
        {
            if (nodeType == this.type && node.Accessible && connectorType == type)
            {
                controlImage.color = ColorDataByType[this.type].ValidControl;
                controlOutline.effectColor = OutlineSelectableColor;
                available = true;
            }
            else
            {
                controlImage.color = ColorDataByType[this.type].InvalidControl;
                controlOutline.effectColor = OutlineUnselectableColor;
                available = false;
            }
        }

        public override void ResetColor()
        {
            controlImage.color = ColorDataByType[type].ValidControl;
            controlOutline.effectColor = OutlineUnselectableColor;
        }

        protected abstract void AddToNode(Node node);
        protected abstract void PrefabSetup();

        protected void SetName(string text)
        {
            // epic line
            this.text.text = text;
            connectorName = text;
        }

        protected void Start()
        {
            allConnectors.Add(this);
        }

        protected void OnDestroy()
        {
            allConnectors.Remove(this);
        }

        private void SetType(Node.Type type)
        {
            this.type = type;
            ResetColor();
        }

        public static Sprite Sprite;

        [SerializeField]
        protected RectTransform rectTransform;
        [SerializeField]
        protected Text text;
        [SerializeField]
        protected Node.Type type;
        [SerializeField]
        protected Image controlImage;
        [SerializeField]
        protected EightSidedOutline controlOutline;

        protected readonly Type connectorType = connectorType;

        public readonly struct Data(Node.Type type, string name)
        {
            public readonly void Setup(Node node)
            {
                var connector = Get(type, name);
                if (connector != null)
                {
                    connector.node = node;
                    connector.AddToNode(node);
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
            image.sprite = Sprite;
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
            connector.text.font = Node.Font;
            connector.text.alignment = TextAnchor.MiddleLeft;
            connector.text.fontSize = 8;
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
            GameObject prefab = type switch
            {
                Node.Type.Float => FloatConnector,
                Node.Type.Integer => IntegerConnector,
                Node.Type.String => StringConnector,
                Node.Type.RDLevelFile => RDLevelConnector,
                _ => null
            };

            if (prefab == null)
            {
                return null;
            }

            var instance = Instantiate(prefab);
            var connector = instance.GetComponent<ConnectorType>();
            connector.SetName(name);

            instance.SetActive(true);
            return connector;
        }

        private readonly Dictionary<Node.Type, ColorData> ColorDataByType = new()
        {
            { Node.Type.Float, Color.green },
            { Node.Type.Integer, Color.blue },
            { Node.Type.String, Color.yellow },
            { Node.Type.RDLevelFile, Color.gray }
        };

        private readonly Color OutlineUnselectableColor = Color.gray;
        private readonly Color OutlineSelectableColor = Color.yellow;

        private readonly record struct ColorData(Color ValidControl)
        {
            public readonly Color InvalidControl = Color.Lerp(ValidControl, Color.black, 0.5f);

            public static implicit operator ColorData(Color SelectedControl) => new(SelectedControl);
        }

        private static GameObject RDLevelConnector
        {
            get
            {
                if (rdlevelConnector == null)
                {
                    rdlevelConnector = SetupConnector(BaseConnectorPrefab, Node.Type.RDLevelFile);
                }

                return rdlevelConnector;
            }
        }

        private static GameObject StringConnector
        {
            get
            {
                if (stringConnector == null)
                {
                    stringConnector = SetupConnector(BaseConnectorPrefab, Node.Type.String);
                }

                return stringConnector;
            }
        }

        private static GameObject IntegerConnector
        {
            get
            {
                if (integerConnector == null)
                {
                    integerConnector = SetupConnector(BaseConnectorPrefab, Node.Type.Integer);
                }

                return integerConnector;
            }
        }

        private static GameObject FloatConnector
        {
            get
            {
                if (floatConnector == null)
                {
                    floatConnector = SetupConnector(BaseConnectorPrefab, Node.Type.Float);
                }

                return floatConnector;
            }
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

        private static GameObject rdlevelConnector;
        private static GameObject stringConnector;
        private static GameObject integerConnector;
        private static GameObject floatConnector;
        private static GameObject baseConnector;
    }
}
