using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types
{
    public abstract class Node_Base : INodeWorkspace.INode
    {
        public void SetSimulator(NodeSimulator simulator)
        {
            this.simulator = simulator;
        }

        public abstract void PostDeserialise();
        public abstract void SetVariable(string name, object value);
        public abstract void SetInput(string name, object value);
        public abstract object GetOutput(string name);

        public static GameObject PreparePrefab(string name) => throw new NotImplementedException();

        protected NodeSimulator simulator;
    }

    public abstract class Node_Base<T> : Node_Base where T : Node_Base<T>
    {
        public override void SetVariable(string name, object value)
        {
            if (!VariableByName.TryGetValue(name, out var variable))
            {
                return;
            }

            if (TryCast(value.ToString(), variable.Type, out object cast))
            {
                variable.Field.SetValue(this, cast);
                return;
            }

            variable.Field.SetValue(this, variable.DefaultValue);
        }

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
            foreach (var variable in VariableFields)
            {
                VariableByName.Add(variable.Name, variable);
            }

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
            var nodeVariables = new NodeVariable.Data[VariableFields.Length];
            var nodeInputs = new NodeInput.Data[InputFields.Length];
            var nodeOutputs = new NodeOutput.Data[OutputFields.Length];

            for (int i = VariableFields.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(VariableFields[i].Type, out Node.Type type))
                {
                    Plugin.LogError($"Type {VariableFields[i].Type} is not recognized as a node type");
                    return null;
                }

                nodeVariables[i] = new(type, VariableFields[i].Name, VariableFields[i].DefaultValue, VariableFields[i].Description);
            }

            for (int i = InputFields.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(InputFields[i].Type, out Node.Type type))
                {
                    Plugin.LogError($"Type {InputFields[i].Type} is not recognized as a node type");
                    return null;
                }

                nodeInputs[i] = new(type, InputFields[i].Name, InputFields[i].Description);
            }

            for (int i = OutputFields.Length - 1; i >= 0; i--)
            {
                if (!TryGetTypeFrom(OutputFields[i].Type, out Node.Type type))
                {
                    Plugin.LogError($"Type {OutputFields[i].Type} is not recognized as a node type");
                    return null;
                }

                nodeOutputs[i] = new(type, OutputFields[i].Name, OutputFields[i].Description);
            }

            return Node.PreparePrefab(name, nodeVariables, nodeInputs, nodeOutputs);
        }

        private static bool TryGetTypeFrom(Type type, out Node.Type result) => Enum.TryParse(type.Name, ignoreCase: true, out result);

        private static bool TryCast(string value, Type type, out object cast)
        {
            if (type == typeof(float))
            {
                if (float.TryParse(value, out var result))
                {
                    cast = result;
                    return true;
                }
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(value, out var result))
                {
                    cast = result;
                    return true;
                }
            }
            else if (type == typeof(string))
            {
                cast = value;
                return true;
            }
            else if (type == typeof(RDLevelFile))
            {
                cast = new RDLevelFile(value);
                return true;
            }
            else if (type == typeof(RDLevelSaveFile))
            {
                cast = new RDLevelSaveFile(value);
                return true;
            }
            else if (type == typeof(bool))
            {
                if (bool.TryParse(value, out var result))
                {
                    cast = result;
                    return true;
                }
            }

            cast = null;
            return false;
        }

        private static readonly FieldInfo[] Fields = typeof(T).GetFields();

        private static readonly Variable[] VariableFields = Fields
            .Select(field => new Variable(field, field.GetCustomAttribute<VariableAttribute>(), GetDescription(field)))
            .Where(input => input.Attribute != null)
            .OrderBy(input => input.Attribute.Order)
            .ToArray();

        private static readonly Input[] InputFields = Fields
            .Select(field => new Input(field, field.GetCustomAttribute<InputAttribute>(), GetDescription(field)))
            .Where(input => input.Attribute != null)
            .OrderBy(input => input.Attribute.Order)
            .ToArray();

        private static readonly Output[] OutputFields = Fields
            .Select(field => new Output(field, field.GetCustomAttribute<OutputAttribute>(), GetDescription(field)))
            .Where(output => output.Attribute != null)
            .OrderBy(output => output.Attribute.Order)
            .ToArray();

        private static readonly Dictionary<string, Variable> VariableByName = new();
        private static readonly Dictionary<string, Input> InputByName = new();
        private static readonly Dictionary<string, Output> OutputByName = new();

        private record Variable(FieldInfo Field, VariableAttribute Attribute, string Description)
        {
            public string Name => Attribute.NameOverride ?? Field.Name;
            public Type Type => Field.FieldType;
            public object DefaultValue => Attribute.DefaultValue;
        }

        private record Input(FieldInfo Field, InputAttribute Attribute, string Description)
        {
            public string Name => Attribute.NameOverride ?? Field.Name;
            public Type Type => Field.FieldType;
        }

        private record Output(FieldInfo Field, OutputAttribute Attribute, string Description)
        {
            public string Name => Attribute.NameOverride ?? Field.Name;
            public Type Type => Field.FieldType;
        }

        private static string GetDescription(FieldInfo field) => field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
    }
}
