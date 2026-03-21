using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Rows
{
    public class Node_MergeRows : Node_Base<Node_MergeRows>
    {
        public override void PostDeserialise()
        {
            var list1 = rows1.Collect();
            int offset = list1.Count;

            var list2 = rows2.Collect();
            foreach (var row in list2)
            {
                row.row += offset;
            }
            list1.AddRange(list2);

            rows = list1;
            events = events2.WithRowOffset(offset);
        }

        [Input]
        public RDLevelRows rows1;

        [Input]
        public RDLevelRows rows2;

        [Input]
        public RDLevelEvents events2;

        [Output]
        public RDLevelRows rows;

        [Output]
        public RDLevelEvents events;
    }
}
