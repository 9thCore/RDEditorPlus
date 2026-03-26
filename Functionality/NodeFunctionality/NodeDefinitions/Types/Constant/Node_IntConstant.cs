using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Constant
{
    public class Node_IntConstant : Node_Base<Node_IntConstant>
    {
        public override void PostDeserialise()
        {
            output = value;
        }

        [Variable<int>]
        public int value;

        [Output("value")]
        public int output;
    }
}
