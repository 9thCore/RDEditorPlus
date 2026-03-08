using RDEditorPlus.Functionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeDefinitions.Types
{
    public abstract class Node_Base : INodeWorkspace.INode
    {
        public abstract void PostDeserialise();
        public abstract void SetInput(string name, object value);
        public abstract object GetOutput(string name);

        public static GameObject PreparePrefab(string name) => throw new NotImplementedException();
    }

    public abstract class Node_Base<InputStorage, OutputStorage> : Node_Base
        where InputStorage : class, new()
        where OutputStorage : class, new()
    {
        public override void SetInput(string name, object value)
        {
            if (InputFieldByName.TryGetValue(name, out var field))
            {
                field.SetValue(input, value);
            }
        }

        public override object GetOutput(string name)
        {
            if (OutputFieldByName.TryGetValue(name, out var field))
            {
                return field.GetValue(output);
            }

            return default;
        }

        protected readonly InputStorage input = new();
        protected readonly OutputStorage output = new();

        public new static GameObject PreparePrefab(string name)
        {
            var nodeInputs = new NodeInput.Data[InputFields.Length];
            var nodeOutputs = new NodeOutput.Data[OutputFields.Length];

            for (int i = InputFields.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(InputFields[i].FieldType, out Node.Type type))
                {
                    Plugin.LogError($"Type {InputFields[i].FieldType} is not recognized as a node type");
                    return null;
                }

                nodeInputs[i] = new(type, InputFields[i].Name);
            }

            for (int i = OutputFields.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(OutputFields[i].FieldType, out Node.Type type))
                {
                    Plugin.LogError($"Type {OutputFields[i].FieldType} is not recognized as a node type");
                    return null;
                }

                nodeOutputs[i] = new(type, OutputFields[i].Name);
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

        static Node_Base()
        {
            foreach (var field in InputFields)
            {
                InputFieldByName.Add(field.Name, field);
            }

            foreach (var field in OutputFields)
            {
                OutputFieldByName.Add(field.Name, field);
            }
        }

        private static readonly FieldInfo[] InputFields = typeof(InputStorage).GetFields();
        private static readonly FieldInfo[] OutputFields = typeof(OutputStorage).GetFields();
        private static readonly Dictionary<string, FieldInfo> InputFieldByName = new();
        private static readonly Dictionary<string, FieldInfo> OutputFieldByName = new();
    }
}
