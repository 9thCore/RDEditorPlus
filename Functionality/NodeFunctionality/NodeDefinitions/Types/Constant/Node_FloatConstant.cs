using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Constant
{
    public class Node_FloatConstant : Node_Base<Node_FloatConstant>
    {
        public override void PostDeserialise()
        {
            output = value;
        }

        [Variable<float>]
        public float value;

        [Output("value")]
        public float output;
    }
}
