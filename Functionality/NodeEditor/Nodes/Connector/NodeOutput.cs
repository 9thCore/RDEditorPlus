using UnityEngine;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes.Connector
{
    public class NodeOutput : NodeConnector<NodeOutput, NodeOutput.Provider>
    {
        public override Type ConnectionType => Type.Output;

        protected override void AddToNode(Node node)
        {
            node.AddOutput(rectTransform);
        }

        protected override void PrefabSetup()
        {
            control.anchorMin = control.anchorMax = new Vector2(1f, 0.5f);

            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = new Vector2(0.84f, 1f);

            text.alignment = TextAnchor.MiddleRight;
        }

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
