using System;

namespace RDEditorPlus.Functionality.NodeDefinitions.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class BaseAttribute(string nameOverride, int order) : Attribute
    {
        public readonly string NameOverride = nameOverride;
        public readonly int Order = order;
    }
}
