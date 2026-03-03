using UnityEngine;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes.Connector
{
    public class NodeInput : NodeConnector<NodeInput, NodeInput.Provider>
    {
        protected override void AddToNode(Node node)
        {
            node.AddInput(rectTransform);
        }

        protected override void PrefabSetup()
        {
            control.anchorMin = control.anchorMax = new Vector2(0f, 0.5f);

            text.rectTransform.anchorMin = new Vector2(0.16f, 0f);
            text.rectTransform.anchorMax = Vector2.one;
        }

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
