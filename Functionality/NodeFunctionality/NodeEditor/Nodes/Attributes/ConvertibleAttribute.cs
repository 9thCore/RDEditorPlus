using System;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Attributes
{
    public class ConvertibleAttribute(Node.TypeConvertible typeConvertible) : Attribute
    {
        public readonly Node.TypeConvertible TypeConvertible = typeConvertible;

        public bool CanDoMath => TypeConvertible.HasFlag(Node.TypeConvertible.Math);
    }
}
