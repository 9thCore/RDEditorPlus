using RDEditorPlus.Functionality.Mixins;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid
{
    public class NodeGrid : MonoBehaviour, IUndoCapable
    {
        public static NodeGrid Create(Transform parent, NodePanelHolder holder)
        {
            GameObject zoomHelper = new(nameof(NodeGrid));
            zoomHelper.SetActive(false);

            zoomHelper.transform.SetParent(parent);
            zoomHelper.transform.localScale = Vector3.one;
            zoomHelper.transform.localPosition = Vector3.zero;

            GameObject root = new("root");

            root.transform.SetParent(zoomHelper.transform);
            root.transform.localScale = Vector3.one;
            root.transform.localPosition = Vector3.zero;

            GameObject background = new("background");

            var image = background.AddComponent<Image>();
            image.sprite = GridSprite;
            image.type = Image.Type.Tiled;

            var rt = background.transform as RectTransform;

            rt.SetParent(root.transform);
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;

            rt.sizeDelta = new Vector2(1900f, 650f);

            GameObject space = new("space");
            var spaceRT = space.AddComponent<RectTransform>();

            spaceRT.SetParent(root.transform);
            spaceRT.localPosition = Vector3.zero;
            spaceRT.localScale = Vector3.one;
            spaceRT.offsetMin = spaceRT.offsetMax = Vector2.zero;

            GameObject connections = new("connections");
            var connectionsRT = connections.AddComponent<RectTransform>();

            connectionsRT.SetParent(root.transform);
            connectionsRT.localPosition = Vector3.zero;
            connectionsRT.localScale = Vector3.one;
            connectionsRT.offsetMin = connectionsRT.offsetMax = Vector2.zero;

            NodeGrid component = root.AddComponent<NodeGrid>();
            root.AddComponent<NodeGridEventTrigger>().Setup(component);
            component.background = rt;
            component.space = spaceRT;
            component.root = root.AddComponent<RectTransform>();
            component.connections = connectionsRT;
            component.holder = holder;
            component.zoomHelper = zoomHelper.AddComponent<RectTransform>();

            zoomHelper.SetActive(true);
            return component;
        }

        public NodeConnection CreateConnection() => NodeConnection.Create(connections);
        public NodeConnection VirtualConnection
        {
            get
            {
                if (virtualConnection == null)
                {
                    virtualConnection = CreateConnection();
                    virtualConnection.gameObject.SetActive(false);
                }

                return virtualConnection;
            }
        }

        public Node AddNode(GameObject prefab, Vector2 position, string id, bool registerUndoEvent = true)
        {
            GameObject instance = Instantiate(prefab, space);

            RectTransform rt = instance.transform as RectTransform;
            rt.transform.localScale = Vector3.one;
            rt.anchoredPosition = position;

            var node = instance.GetComponent<Node>();
            string nodeID = node.GenerateID(id);
            node.Setup(this);
            nodes.Add(node);

            instance.SetActive(true);

            if (registerUndoEvent)
            {
                RegisterAction(new AddNodeAction(this, prefab, nodeID, position));
            }

            return node;
        }

        public void AddNode(string name, Vector2 position) => holder.AddNode(name, position);

        public void DeleteNode(Node node)
        {
            nodes.Remove(node);
        }

        public void AddNodeAtPointerPosition(string name, Vector2 position)
        {
            AddNode(name, (position - transform.position.xy()) / transform.lossyScale.x);
        }

        public void Drag(Vector2 delta)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            root.anchoredPosition += delta / root.lossyScale;
            UpdateBackground();
        }

        public void Reset()
        {
            root.anchoredPosition = Vector2.zero;
            zoomHelper.localScale = Vector3.one;
            UpdateBackground();
        }

        public async Task SaveAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(NodesKey);

            var orderedNodes = GetDependencyOrderedNodes();
            if (orderedNodes == null)
            {
                Plugin.LogError("Something went wrong while saving nodes, as we did not save anything for an iteration. Exitting node saving early");
                await writer.WriteEndElementAsync();
                return;
            }

            foreach (var node in orderedNodes)
            {
                await writer.WriteStartElementAsync(NodeKey);
                await node.SaveAsync(writer);
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        }

        public void Clear()
        {
            foreach (var node in nodes)
            {
                node.Delete(dontDeleteFromGrid: true);
            }

            nodes.Clear();
        }

        public void SetAllNodesAsAccessible()
        {
            foreach (var node in nodes)
            {
                node.Accessible = true;
            }
        }

        public bool TryGetNodeFromID(string id, out Node result)
        {
            foreach (var node in nodes)
            {
                if (node.Id == id)
                {
                    result = node;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public Node[] GetDependencyOrderedNodes()
        {
            foreach (var node in nodes)
            {
                node.InputDependenciesSaved = false;
                node.AlreadySaved = false;
            }

            int length = nodes.Count;
            var orderedNodes = new Node[length];
            int index = 0;

            while (index < length)
            {
                bool savedAnythingThisIteration = false;

                foreach (var node in nodes)
                {
                    if (!node.AlreadySaved && node.InputDependenciesSaved)
                    {
                        orderedNodes[index++] = node;
                        node.PropagateDependenciesSaved();
                        node.AlreadySaved = true;
                        savedAnythingThisIteration = true;
                    }
                }

                if (!savedAnythingThisIteration)
                {
                    return null;
                }
            }

            return orderedNodes;
        }

        public void RegisterMoveNodeAction(Node node, Vector3 oldPosition, Vector3 currentPosition)
            => RegisterAction(new MoveNodeAction(this, node.Id, (currentPosition - oldPosition).xy()));

        public void Undo() => this.DefaultUndo();
        public void Redo() => this.DefaultRedo();
        public void ClearUndo() => this.DefaultClearUndo();
        public void RegisterAction(IUndoCapable.IAction action) => this.DefaultRegisterAction(action);

        public Stack<IUndoCapable.IAction> UndoStack { get; init; } = new();
        public Stack<IUndoCapable.IAction> RedoStack { get; init; } = new();
        public bool CanZoom { get; set; } = false;

        private void UpdateBackground()
        {
            background.anchoredPosition = -root.anchoredPosition.RoundToMultiple(GridSize);
        }

        private void Zoom(float delta, Vector2 mousePosition)
        {
            if (delta == 0f)
            {
                return;
            }

            Vector3 position = root.position;
            zoomHelper.position = mousePosition;
            root.position = position;

            float zoom = Mathf.Clamp(CurrentZoom + delta, MinZoom, MaxZoom);
            zoomHelper.localScale = Vector3.one * zoom;

            UpdateBackground();
        }

        private void Update()
        {
            if (!CanZoom || !RDEditorUtils.ControlIsPressed())
            {
                return;
            }

            Zoom(Input.mouseScrollDelta.y * ZoomSpeedFactor, Input.mousePosition);
        }

        private float CurrentZoom => zoomHelper.localScale.x;

        private RectTransform zoomHelper;
        private RectTransform root;
        private RectTransform space;
        private RectTransform background;
        private RectTransform connections;
        private NodeConnection virtualConnection;
        private NodePanelHolder holder;
        private readonly List<Node> nodes = new();

        public const string NodesKey = "Nodes";
        public const string NodeKey = "Node";
        
        private const float ZoomSpeedFactor = 10f / 60f;
        private const float MinZoom = 0.33f;
        private const float MaxZoom = 3.0f;

        public static Sprite GridSprite
        {
            get
            {
                if (gridSprite == null)
                {
                    Texture2D texture = new(GridSize, GridSize);
                    Color main = Color.gray;
                    Color background = Color.black;

                    for (int i = 0; i < GridSize; i++)
                    {
                        for (int j = 0; j < GridSize; j++)
                        {
                            texture.SetPixel(j, i, background);
                        }
                    }

                    for (int i = 0; i < GridSize; i++)
                    {
                        texture.SetPixel(0, i, main);
                        texture.SetPixel(i, 0, main);
                    }

                    texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
                    gridSprite = Sprite.Create(texture, new Rect(0, 0, GridSize, GridSize), Vector2.zero);
                }

                return gridSprite;
            }
        }

        public const int GridSize = 6;
        private static Sprite gridSprite = null;

        private record AddNodeAction(NodeGrid Grid, GameObject Prefab, string ID, Vector3 Position) : NodeTargettableAction(Grid, ID)
        {
            public override void OnRedo() => Grid.AddNode(Prefab, Position, ID, registerUndoEvent: false);
            public override void OnUndo() => GetNode(node => node.Delete(dontDeleteFromGrid: false));
        }

        private record MoveNodeAction(NodeGrid Grid, string ID, Vector2 DeltaPosition) : NodeTargettableAction(Grid, ID)
        {
            public override void OnRedo() => GetNode(node => node.Drag(DeltaPosition));
            public override void OnUndo() => GetNode(node => node.Drag(-DeltaPosition));
        }

        private abstract record NodeTargettableAction(NodeGrid Grid, string ID) : IUndoCapable.IAction
        {
            public abstract void OnRedo();
            public abstract void OnUndo();

            protected void GetNode(Action<Node> callback)
            {
                if (!TryGetNode(out var result))
                {
                    Plugin.LogWarn($"Tried to undo {this}, but there was no node");
                    return;
                }

                callback(result);
            }

            protected bool TryGetNode(out Node result) => Grid.TryGetNodeFromID(ID, out result);
        }
    }
}
