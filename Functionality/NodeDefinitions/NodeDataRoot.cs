using RDEditorPlus.Functionality.NodeEditor;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeDefinitions
{
    public class NodeDataRoot
    {
        public virtual void Deserialize(NodePanelHolder holder)
        {
            int index = 0;
            foreach (var nodeData in Nodes)
            {
                if (nodeData.id.IsNullOrEmpty())
                {
                    throw new InvalidDataException($"Node #{index} is missing an ID");
                }

                var node = holder.AddNode(nodeData.name, nodeData.Position ?? Vector2.zero, nodeData.id);
                if (node == null)
                {
                    throw new InvalidDataException();
                }

                if (nodeData.Inputs != null)
                {
                    foreach (var inputData in nodeData.Inputs)
                    {
                        var link = inputData.Link;
                        if (link != null)
                        {
                            if (!holder.TryGetNodeFromID(link.target, out var other))
                            {
                                throw new InvalidDataException($"Node {link.target} does not exist before {nodeData.id} needs it. Definition order of nodes in the save matters");
                            }

                            var input = node.GetInput(inputData.name);
                            var output = other.GetOutput(link.output);
                            if (input != null && output != null)
                            {
                                input.LinkIfNotYetLinked(output);
                            }
                        }
                    }
                }

                index++;
            }
        }

        public Node[] Nodes;

        public class Node
        {
            [XmlAttribute]
            public string id, name;

            public Position Position;

            public Input[] Inputs;
        }

        public class Position
        {
            [XmlAttribute]
            public float x, y;

            public static implicit operator Vector2(Position p) => new(p.x, p.y);
        }

        public class Input
        {
            [XmlAttribute]
            public string name;

            public Link Link;
        }

        public class Link
        {
            [XmlAttribute]
            public string target, output;
        }
    }
}
