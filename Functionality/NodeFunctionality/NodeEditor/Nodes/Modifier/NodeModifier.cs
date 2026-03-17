using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Modifier
{
    public abstract class NodeModifier : MonoBehaviour
    {
        public void Setup(Node node)
        {
            this.node = node;
        }

        [SerializeField]
        protected Node node;
    }
}
