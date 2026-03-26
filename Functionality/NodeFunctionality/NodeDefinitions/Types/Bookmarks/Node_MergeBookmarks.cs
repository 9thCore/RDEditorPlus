using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Bookmarks
{
    public class Node_MergeBookmarks : Node_Base<Node_MergeBookmarks>
    {
        public override void PostDeserialise()
        {
            var list = input1.Collect();
            list.AddRange(input2.Collect());
            output = list;
        }

        [Input]
        public RDLevelBookmarks input1;

        [Input]
        public RDLevelBookmarks input2;

        [Output]
        public RDLevelBookmarks output;
    }
}
