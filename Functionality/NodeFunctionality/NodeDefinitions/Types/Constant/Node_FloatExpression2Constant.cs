using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Constant
{
    internal class Node_FloatExpression2Constant : Node_Base<Node_FloatExpression2Constant>
    {
        public override void PostDeserialise()
        {
            output = value;
        }

        [Variable]
        public FloatExpression2 value;

        [Output("value")]
        public FloatExpression2 output;
    }
}
