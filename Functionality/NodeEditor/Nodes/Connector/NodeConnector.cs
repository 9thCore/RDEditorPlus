using RDEditorPlus.Functionality.Components;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes.Connector
{
    public abstract class NodeConnector : MonoBehaviour
    {
        public abstract void StartConnection();
        public abstract void UpdateConnection(Vector2 pointerPosition);
        public abstract void EndConnection(Vector2 pointerPosition);
    }

    public abstract class NodeConnector<ConnectorType, Provider> : NodeConnector where ConnectorType : NodeConnector<ConnectorType, Provider> where Provider : IPrefabProvider, new()
    {
        public abstract Type ConnectionType { get; }

        public override void StartConnection()
        {
            node.VirtualConnection.SetAnchor(control);
            node.VirtualConnection.gameObject.SetActive(true);
        }

        public override void UpdateConnection(Vector2 pointerPosition)
        {
            node.VirtualConnection.SetEndPoint(pointerPosition);
        }

        public override void EndConnection(Vector2 pointerPosition)
        {
            node.VirtualConnection.gameObject.SetActive(false);
        }

        protected abstract void AddToNode(Node node);
        protected abstract void PrefabSetup();

        protected void SetName(string text)
        {
            // epic line
            this.text.text = text;
        }

        public static Sprite Sprite;

        [SerializeField]
        protected RectTransform rectTransform;

        [SerializeField]
        protected RectTransform control;

        [SerializeField]
        protected Text text;

        [SerializeField]
        protected Node.Type type;

        [SerializeField]
        protected Node node;

        public readonly struct Data(Node.Type type, string name)
        {
            public readonly Node.Type type = type;
            public readonly string name = name;

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

        public enum Type
        {
            Input,
            Output
        }

        protected static GameObject BaseConnector(string connectorType)
        {
            GameObject baseConnector = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_Node{connectorType}");
            baseConnector.SetActive(false);

            baseConnector.AddComponent<LayoutElement>();

            var rt = baseConnector.transform as RectTransform;

            GameObject control = new("control");

            control.AddComponent<EightSidedOutline>().effectColor = Color.gray;

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
            return baseConnector;
        }

        protected static GameObject SetupFloatConnector(GameObject baseConnector)
        {
            GameObject floatConnector = GameObject.Instantiate(baseConnector);

            floatConnector.name = $"{baseConnector.name}Float";
            floatConnector.GetComponent<ConnectorType>().type = Node.Type.Float;
            return floatConnector;
        }

        private static NodeConnector<ConnectorType, Provider> Get(Node.Type type, string name)
        {
            GameObject prefab = new Provider().GetPrefab(type);

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
    }
}
