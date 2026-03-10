
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeDefinitions
{
    public interface INodeWorkspace
    {
        public INode AddNode(string name, Vector2 position, string id);
        public bool TryGetNodeFromID(string id, out INode node);
        public void HandleLink(INode input, string inputName, INode output, string outputName);

        public interface INode
        {
            public void PostDeserialise();
            public void SetVariable(string name, object value);
        }
    }
}
