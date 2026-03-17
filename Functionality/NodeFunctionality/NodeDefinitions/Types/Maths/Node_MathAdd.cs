using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes.Modifier;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Modifier;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Maths
{
    [NodeModifier<MathNodeModifier>]
    public class Node_MathAdd : Node_Base<Node_MathAdd>
    {
        public override void PostDeserialise()
        {
            value = value1 + value2;
            Plugin.LogInfo(value);
        }

        [Input]
        public MathConvertible value1 = new();

        [Input]
        public MathConvertible value2 = new();

        [Output]
        public MathConvertible value;
    }
}
