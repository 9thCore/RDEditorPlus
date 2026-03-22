using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelConditionals(List<Conditional> conditionals)
    {
        public RDLevelConditionals() : this(null) { }

        public List<Conditional> Collect() => this;

        public readonly IReadOnlyList<Conditional> Conditionals = conditionals.AsReadOnly();

        public static implicit operator List<Conditional>(RDLevelConditionals instance)
            => instance.Conditionals != null ? [.. instance.Conditionals] : [];

        public static implicit operator RDLevelConditionals(List<Conditional> list) => new(list);
    }
}
