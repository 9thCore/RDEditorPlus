using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using RDLevelEditor;

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

            var conds = conditionals2.Collect();
            foreach (var cond in conds)
            {
                if (cond is Conditional_LastHit lastHit)
                {
                    lastHit.row += offset;
                }
            }

            rows = list1;
            events = events2.WithRowOffset(offset);
            conditionals = conds;
        }

        [Input]
        public RDLevelRows rows1;

        [Input]
        public RDLevelRows rows2;

        [Input]
        public RDLevelEvents events2;

        [Input]
        public RDLevelConditionals conditionals2;

        [Output]
        public RDLevelRows rows;

        [Output]
        public RDLevelEvents events;

        [Output]
        public RDLevelConditionals conditionals;
    }
}
