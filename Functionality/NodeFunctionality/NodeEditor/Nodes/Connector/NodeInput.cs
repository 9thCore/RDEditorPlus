using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Util;
using System;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector
{
    public class NodeInput : NodeConnector<NodeInput, NodeInput.PrefabProvider>, ISerializableNodeWorkspace.INode.IInput
    {
        public class PrefabProvider : IPrefabProvider
        {
            public Type ConnectorType => Type.Input;
        }

        private NodeInput() : base(Type.Input)
        {

        }

        public void SetLink(Link link, bool replace)
        {
            if (this.link != null)
            {
                this.link.Unlink(replace);
            }

            this.link = link;
        }

        public void RemoveLink()
        {
            link = null;
        }

        public void Drag()
        {
            if (link != null)
            {
                link.Connection.SetEndPoint(control.position.xy());
            }
        }

        protected override void AddToNode(Node node, string description)
        {
            node.AddInput(rectTransform, this, description);
        }

        public override void Unlink(bool dontRaiseDisconnectEvent)
        {
            if (link != null)
            {
                link.Unlink(dontRaiseDisconnectEvent);
            }
        }

        public override void UndoableUnlinkAll()
        {
            node.SendInputUnlinkEvent(this, NodeTarget);
            Unlink(dontRaiseDisconnectEvent: false);
        }

        public override void PropagateInaccessibility()
        {
            if (link != null)
            {
                link.Output.node.PropagateInaccessibilityThroughInputs();
            }
        }

        public override bool ConnectedTo(NodeConnector connector)
        {
            return link != null && link.Output == connector;
        }

        public override bool ConnectedToAnything => link != null;

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

            output.SetupConnection(this, justReplace: false);
        }

        public bool IsLinked => link != null;
        public string Target => link.Output.Id;
        public string Output => link.Output.Name;
        public NodeOutput ConnectedOutput => IsLinked ? link.Output : null;
        public NodeGrid.NodeTarget NodeTarget => IsLinked ? new NodeGrid.NodeTarget(Target, Output) : new NodeGrid.NodeTarget(null, null);

        protected override void PrefabSetup()
        {
            control.anchorMin = control.anchorMax = new Vector2(0f, 0.5f);

            text.rectTransform.anchorMin = new Vector2(0.16f, 0f);
            text.rectTransform.anchorMax = Vector2.one;
        }

        protected internal override void SetupConnection(NodeConnector other, bool justReplace) => throw new NotImplementedException();

        public const string LinkKey = "Link";

        private Link link;
        private bool dependenciesSaved = false;
    }
}
