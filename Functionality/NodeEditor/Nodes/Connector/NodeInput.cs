using RDEditorPlus.Functionality.NodeDefinitions;
using RDEditorPlus.Util;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes.Connector
{
    public class NodeInput : NodeConnector<NodeInput, NodeInput.Provider>, ISerializableNodeWorkspace.INode.IInput
    {
        private NodeInput() : base(Type.Input)
        {

        }

        public void SetLink(Link link)
        {
            if (this.link != null)
            {
                this.link.Unlink();
            }

            this.link = link;
        }

        public void RemoveLink()
        {
            link = null;
        }

        public void Drag(Vector2 delta)
        {
            if (link != null)
            {
                link.Connection.OffsetEndPoint(delta);
            }
        }

        protected override void AddToNode(Node node)
        {
            node.AddInput(rectTransform, this);
        }

        public override void Unlink()
        {
            if (link != null)
            {
                link.Unlink();
            }
        }

        public override void PropagateInaccessibility()
        {
            if (link != null)
            {
                link.Output.node.PropagateInaccessibilityThroughInputs();
            }
        }

        public async Task SaveAsync(XmlWriter writer)
        {
            await writer.WriteAttributeStringAsync(NameKey, connectorName);

            await writer.WriteStartElementAsync(LinkKey);
            await link.SaveAsync(writer);
            await writer.WriteEndElementAsync();
        }

        public bool CanSave()
        {
            return link != null;
        }

        public bool DependenciesSaved
        {
            get => link == null || dependenciesSaved;
            set => dependenciesSaved = value;
        }

        public void LinkIfNotYetLinked(NodeOutput output)
        {
            if (link != null)
            {
                Plugin.LogInfo($"Node({Id}).{connectorName} already connected to Node({link.Output.Id}), skipping new connection");
                return;
            }

            output.SetupConnection(this);
        }

        public bool IsLinked => link != null;
        public string Target => link.Output.Id;
        public string Output => link.Output.Name;

        protected override void PrefabSetup()
        {
            control.anchorMin = control.anchorMax = new Vector2(0f, 0.5f);

            text.rectTransform.anchorMin = new Vector2(0.16f, 0f);
            text.rectTransform.anchorMax = Vector2.one;
        }

        protected internal override void SetupConnection(NodeConnector other)
        {
            other.SetupConnection(this);
        }

        public const string LinkKey = "Link";

        public class Provider : IPrefabProvider
        {
            public GameObject GetPrefab(Node.Type type)
            {
                return type switch
                {
                    Node.Type.Float => FloatInput,
                    Node.Type.Integer => IntegerInput,
                    _ => null
                };
            }
        }

        private Link link;
        private bool dependenciesSaved = false;

        private static GameObject IntegerInput
        {
            get
            {
                if (integerInput == null)
                {
                    integerInput = SetupIntegerConnector(BaseInput);
                }

                return integerInput;
            }
        }

        private static GameObject FloatInput
        {
            get
            {
                if (floatInput == null)
                {
                    floatInput = SetupFloatConnector(BaseInput);
                }

                return floatInput;
            }
        }

        private static GameObject BaseInput
        {
            get
            {
                if (baseInput == null)
                {
                    baseInput = BaseConnector("input");
                }

                return baseInput;
            }
        }

        private static GameObject integerInput;
        private static GameObject floatInput;
        private static GameObject baseInput;
    }
}
