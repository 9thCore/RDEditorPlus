using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Constant
{
    internal class Node_FloatExpressionConstant : Node_Base<Node_FloatExpressionConstant>
    {
        public override void PostDeserialise()
        {
            output = value;
        }

        [Variable]
        public FloatExpression value;

        [Output("value")]
        public FloatExpression output;
    }
}
