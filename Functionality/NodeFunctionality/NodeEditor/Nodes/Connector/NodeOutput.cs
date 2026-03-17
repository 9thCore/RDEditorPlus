using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector
{
    public class NodeOutput : NodeConnector<NodeOutput, NodeOutput.PrefabProvider>
    {
        public class PrefabProvider : IPrefabProvider
        {
            public Type ConnectorType => Type.Output;
        }

        private NodeOutput() : base(Type.Output)
        {

        }

        public void Remove(Link link)
        {
            links.Remove(link);
        }

        public void Drag()
        {
            foreach (var link in links)
            {
                link.Connection.SetAnchor(control);
                link.Connection.SetEndPoint(link.Input.control.position.xy());
            }
        }

        protected override void AddToNode(Node node, string description)
        {
            node.AddOutput(rectTransform, this, description);
        }

        public override void Unlink(bool dontRaiseDisconnectEvent)
        {
            List<Link> toBeRemoved = new(links);

            foreach (var link in toBeRemoved)
            {
                link.Unlink(dontRaiseDisconnectEvent);
            }
        }

        public override void PropagateInaccessibility()
        {
            foreach (var link in links)
            {
                link.Input.node.PropagateInaccessibilityThroughOutputs();
            }
        }

        public override bool ConnectedTo(NodeConnector connector)
        {
            foreach (var link in links)
            {
                if (link.Input == connector)
                {
                    return true;
                }
            }

            return false;
        }

        public override bool ConnectedToAnything => links.Count > 0;

        public void PropagateDependenciesSaved()
        {
            foreach (var link in links)
            {
                link.Input.DependenciesSaved = true;
            }
        }

        protected override void PrefabSetup()
        {
            control.anchorMin = control.anchorMax = new Vector2(1f, 0.5f);

            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = new Vector2(0.84f, 1f);

            text.alignment = TextAnchor.MiddleRight;
        }

        protected internal override void SetupConnection(NodeConnector other, bool justReplace)
        {
            var input = other as NodeInput;

            var connection = node.CreateConnection();
            connection.SetAnchor(control);
            connection.SetEndPoint(other.control.position.xy());
            Link link = new(input, this, connection);

            links.Add(link);
            input.SetLink(link, justReplace);

            Node.Type type = colorOverrideType ?? this.type;

            if (type == Node.Type.MathConvertible || Node.CanConvertToMath(type))
            {
                input.SetColorOverrideType(type);
            }

            if (justReplace)
            {
                input.node.RaiseReplaceEvent();
                node.RaiseReplaceEvent();
            }
            else
            {
                input.node.RaiseConnectEvent();
                node.RaiseConnectEvent();
            }
        }

        private readonly List<Link> links = new();
    }
}
