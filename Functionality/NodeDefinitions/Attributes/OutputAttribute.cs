using System;
using System.Runtime.CompilerServices;

namespace RDEditorPlus.Functionality.NodeDefinitions.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputAttribute(string nameOverride = null, [CallerLineNumber] int order = 0) : Attribute
    {
        public readonly string NameOverride = nameOverride;
        public readonly int Order = order;
    }
}
