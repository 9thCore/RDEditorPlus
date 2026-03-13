using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelRows(List<LevelEvent_MakeRow> rows)
    {
        public RDLevelRows() : this(null) { }

        public readonly IReadOnlyList<LevelEvent_MakeRow> Rows = rows.AsReadOnly();

        public static implicit operator List<LevelEvent_MakeRow>(RDLevelRows instance)
            => instance.Rows != null ? [.. instance.Rows] : [];

        public static implicit operator RDLevelRows(List<LevelEvent_MakeRow> list) => new(list);
    }
}
