using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeDefinitions
{
    public class NodeDataRoot
    {
        public virtual void Deserialize(INodeWorkspace workspace)
        {
            int index = 0;
            foreach (var nodeData in Nodes)
            {
                if (nodeData.id.IsNullOrEmpty())
                {
                    throw new InvalidDataException($"Node #{index} is missing an ID");
                }

                var node = workspace.AddNode(nodeData.name, nodeData.Position ?? Vector2.zero, nodeData.id);
                if (node == null)
                {
                    throw new InvalidDataException();
                }

                if (nodeData.Variables != null)
                {
                    foreach (var variableData in nodeData.Variables)
                    {
                        node.SetVariable(variableData.name, variableData.Value);
                    }
                }

                if (nodeData.Inputs != null)
                {
                    foreach (var inputData in nodeData.Inputs)
                    {
                        var link = inputData.Link;
                        if (link != null)
                        {
                            if (!workspace.TryGetNodeFromID(link.target, out var other))
                            {
                                throw new InvalidDataException($"Node {link.target} does not exist before {nodeData.id} needs it. Definition order of nodes in the save matters");
                            }

                            workspace.HandleLink(node, inputData.name, other, link.output);
                        }
                    }
                }

                node.PostDeserialise();
                index++;
            }
        }

        public virtual void Serialize(ISerializableNodeWorkspace workspace)
        {
            var orderedNodes = workspace.GetDependencyOrderedNodes();
            int nodeCount = orderedNodes.Length;

            Nodes = new Node[nodeCount];

            for (int i = 0; i < nodeCount; i++)
            {
                var node = orderedNodes[i];
                var variables = node.Variables;
                var inputs = node.Inputs;
                int variableCount = variables.Length;
                int inputCount = inputs.Length;
                
                Node serialisedNode = new()
                {
                    id = node.Id,
                    name = node.Name,
                    Position = node.Position,
                    Variables = new Variable[variableCount],
                    Inputs = new Input[inputCount]
                };

                for (int j = 0; j < variableCount; j++)
                {
                    var variable = variables[j];

                    serialisedNode.Variables[j] = new()
                    {
                        name = variable.Name,
                        Value = variable.Value.ToString()
                    };
                }

                for (int j = 0; j < inputCount; j++)
                {
                    var input = inputs[j];

                    Input serialisedInput = new()
                    {
                        name = input.Name,
                        Link = null
                    };

                    if (input.IsLinked)
                    {
                        serialisedInput.Link = new()
                        {
                            target = input.Target,
                            output = input.Output
                        };
                    }

                    serialisedNode.Inputs[j] = serialisedInput;
                }

                Nodes[i] = serialisedNode;
            }
        }

        public Node[] Nodes;

        public class Node
        {
            [XmlAttribute]
            public string id, name;

            public Position Position;
            public Variable[] Variables;
            public Input[] Inputs;
        }

        public class Position
        {
            [XmlAttribute]
            public float x, y;

            public static implicit operator Vector2(Position p) => new(p.x, p.y);
            public static implicit operator Position(Vector2 v) => new() { x = v.x, y = v.y };
        }

        public class Variable
        {
            [XmlAttribute]
            public string name;

            public string Value;
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
