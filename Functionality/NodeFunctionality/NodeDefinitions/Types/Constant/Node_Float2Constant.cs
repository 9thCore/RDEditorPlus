using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Constant
{
    internal class Node_Float2Constant : Node_Base<Node_Float2Constant>
    {
        public override void PostDeserialise()
        {
            output = value;
        }

        [Variable]
        public Float2 value = new();

        [Output("value")]
        public Float2 output;
    }
}
