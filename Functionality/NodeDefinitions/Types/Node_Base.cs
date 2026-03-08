using RDEditorPlus.Functionality.NodeDefinitions.Attributes;
using RDEditorPlus.Functionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeDefinitions.Types
{
    public abstract class Node_Base : INodeWorkspace.INode
    {
        public abstract void PostDeserialise();
        public abstract void SetInput(string name, object value);
        public abstract object GetOutput(string name);

        public static GameObject PreparePrefab(string name) => throw new NotImplementedException();
    }

    public abstract class Node_Base<T> : Node_Base where T : Node_Base<T>
    {
        public override void SetInput(string name, object value)
        {
            if (!InputByName.TryGetValue(name, out var input))
            {
                return;
            }

            input.Field.SetValue(this, value);
        }

        public override object GetOutput(string name)
        {
            if (!OutputByName.TryGetValue(name, out var output))
            {
                return null;
            }

            return output.Field.GetValue(this);
        }

        static Node_Base()
        {
            foreach (var input in InputFields)
            {
                InputByName.Add(input.Name, input);
            }

            foreach (var output in OutputFields)
            {
                OutputByName.Add(output.Name, output);
            }
        }

        public new static GameObject PreparePrefab(string name)
        {
            var nodeInputs = new NodeInput.Data[InputFields.Length];
            var nodeOutputs = new NodeOutput.Data[OutputFields.Length];

            for (int i = InputFields.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(InputFields[i].Type, out Node.Type type))
                {
                    Plugin.LogError($"Type {InputFields[i].Type} is not recognized as a node type");
                    return null;
                }

                nodeInputs[i] = new(type, InputFields[i].Name);
            }

            for (int i = OutputFields.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(OutputFields[i].Type, out Node.Type type))
                {
                    Plugin.LogError($"Type {OutputFields[i].Type} is not recognized as a node type");
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

        private static readonly FieldInfo[] Fields = typeof(T).GetFields();

        private static readonly Input[] InputFields = Fields
            .Select(field => new Input(field, field.GetCustomAttribute<InputAttribute>()))
            .Where(input => input.Attribute != null)
            .OrderBy(input => input.Attribute.Order)
            .ToArray();

        private static readonly Output[] OutputFields = Fields
            .Select(field => new Output(field, field.GetCustomAttribute<OutputAttribute>()))
            .Where(output => output.Attribute != null)
            .OrderBy(output => output.Attribute.Order)
            .ToArray();

        private static readonly Dictionary<string, Input> InputByName = new();
        private static readonly Dictionary<string, Output> OutputByName = new();

        private record Input(FieldInfo Field, InputAttribute Attribute)
        {
            public string Name => Attribute.NameOverride ?? Field.Name;
            public Type Type => Field.FieldType;
        }

        private record Output(FieldInfo Field, OutputAttribute Attribute)
        {
            public string Name => Attribute.NameOverride ?? Field.Name;
            public Type Type => Field.FieldType;
        }
    }
}
