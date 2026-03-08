using RDEditorPlus.Functionality.NodeDefinitions.Types;
using System.Collections.Generic;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeDefinitions
{
    public class NodeSimulator : INodeWorkspace
    {
        public INodeWorkspace.INode AddNode(string name, Vector2 _, string id)
        {
            if (!NodeLibrary.Instance.TryGetInstance(name, out var instance))
            {
                return null;
            }

            nodes.Add(id, instance);
            return instance;
        }

        public bool TryGetNodeFromID(string id, out INodeWorkspace.INode node)
        {
            if (!nodes.TryGetValue(id, out var nodeBase))
            {
                node = null;
                return false;
            }

            node = nodeBase;
            return true;
        }

        public void HandleLink(INodeWorkspace.INode input, string inputName, INodeWorkspace.INode output, string outputName)
        {
            var inputNode = (Node_Base)input;
            var outputNode = (Node_Base)output;

            inputNode.SetInput(inputName, outputNode.GetOutput(outputName));
        }

        private readonly Dictionary<string, Node_Base> nodes = new();
    }
}
