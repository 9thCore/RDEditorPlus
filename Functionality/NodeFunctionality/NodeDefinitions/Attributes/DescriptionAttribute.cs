using System;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes
{
    public class DescriptionAttribute(string description) : Attribute
    {
        public readonly string Description = description;
    }
}
