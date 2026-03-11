using System.Runtime.CompilerServices;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes
{
    public class VariableAttribute(object defaultValue = null, string nameOverride = null, [CallerLineNumber] int order = 0)
        : BaseAttribute(nameOverride, order)
    {
        public readonly object DefaultValue = defaultValue;
    }

    public class VariableAttribute<T>(T defaultValue = default, string nameOverride = null, [CallerLineNumber] int order = 0)
        : VariableAttribute(defaultValue, nameOverride, order)
    { }
}
