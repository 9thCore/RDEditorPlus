using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Events
{
    public class Node_MergeEvents : Node_Base<Node_MergeEvents>
    {
        public override void PostDeserialise()
        {
            var list = input1.Apply();
            list.AddRange(input2.Apply());
            output = new(list);
        }

        [Input]
        public RDLevelEvents input1;

        [Input]
        public RDLevelEvents input2;

        [Output]
        public RDLevelEvents output;
    }
}
