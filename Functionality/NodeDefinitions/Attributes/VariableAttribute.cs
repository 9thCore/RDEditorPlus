using System.Runtime.CompilerServices;

namespace RDEditorPlus.Functionality.NodeDefinitions.Attributes
{
    public abstract class VariableAttribute(object defaultValue, string nameOverride, int order)
        : BaseAttribute(nameOverride, order)
    {
        public readonly object DefaultValue = defaultValue;
    }

    public class VariableAttribute<T>(T defaultValue = default, string nameOverride = null, [CallerLineNumber] int order = 0)
        : VariableAttribute(defaultValue, nameOverride, order)
    { }
}
