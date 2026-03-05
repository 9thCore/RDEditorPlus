using System.Collections.Generic;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes.Connector
{
    public class NodeOutput : NodeConnector<NodeOutput, NodeOutput.Provider>
    {
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

        protected override void AddToNode(Node node)
        {
            node.AddOutput(rectTransform, this);
        }

        protected override void PrefabSetup()
        {
            control.anchorMin = control.anchorMax = new Vector2(1f, 0.5f);

            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = new Vector2(0.84f, 1f);

            text.alignment = TextAnchor.MiddleRight;
        }

        protected internal override void SetupConnection(NodeConnector other)
        {
            var input = other as NodeInput;

            var connection = node.CreateConnection();
            connection.SetAnchor(control);
            connection.SetEndPoint(other.control.position.xy());
            Link link = new(input, this, connection);

            links.Add(link);
            input.SetLink(link);
        }

        private readonly List<Link> links = new();

        public class Provider : IPrefabProvider
        {
            public GameObject GetPrefab(Node.Type type)
            {
                return type switch
                {
                    Node.Type.Float => FloatOutput,
                    _ => null
                };
            }
        }

        private static GameObject FloatOutput
        {
            get
            {
                if (floatOutput == null)
                {
                    floatOutput = SetupFloatConnector(BaseOutput);
                }

                return floatOutput;
            }
        }

        private static GameObject BaseOutput
        {
            get
            {
                if (baseOutput == null)
                {
                    baseOutput = BaseConnector("output");
                }

                return baseOutput;
            }
        }

        private static GameObject floatOutput;
        private static GameObject baseOutput;
    }
}
