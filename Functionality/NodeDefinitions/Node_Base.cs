using RDEditorPlus.Functionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using System;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeDefinitions
{
    public abstract class Node_Base
    {
        public static GameObject PreparePrefab(string name) => throw new NotImplementedException();
    }

    public abstract class Node_Base<InputStorage, OutputStorage> : Node_Base
    {
        public new static GameObject PreparePrefab(string name)
        {
            var inputs = typeof(InputStorage).GetFields();
            var outputs = typeof(OutputStorage).GetFields();

            var nodeInputs = new NodeInput.Data[inputs.Length];
            var nodeOutputs = new NodeOutput.Data[outputs.Length];

            for (int i = inputs.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(inputs[i].FieldType, out Node.Type type))
                {
                    Plugin.LogError($"Type {inputs[i].FieldType} is not recognized as a node type");
                    return null;
                }

                nodeInputs[i] = new(type, inputs[i].Name);
            }

            for (int i = outputs.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(outputs[i].FieldType, out Node.Type type))
                {
                    Plugin.LogError($"Type {outputs[i].FieldType} is not recognized as a node type");
                    return null;
                }

                nodeOutputs[i] = new(type, outputs[i].Name);
            }

            return Node.PreparePrefab(name, nodeInputs, nodeOutputs);
        }

        private static bool TryGetTypeFrom(Type type, out Node.Type result)
        {
            if (type == typeof(float))
            {
                result = Node.Type.Float;
                return true;
            }

            result = default;
            return false;
        }
    }
}
