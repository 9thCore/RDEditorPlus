using UnityEngine;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes.Connector
{
    public class NodeInput : NodeConnector<NodeInput, NodeInput.Provider>
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
            link.Unlink();
        }

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

        private Link link;

        public class Provider : IPrefabProvider
        {
            public GameObject GetPrefab(Node.Type type)
            {
                return type switch
                {
                    Node.Type.Float => FloatInput,
                    _ => null
                };
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

        private static GameObject floatInput;
        private static GameObject baseInput;
    }
}
