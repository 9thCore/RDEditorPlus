using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes.Modifier;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Modifier;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Maths
{
    [NodeModifier<MathNodeModifier>]
    public class Node_MathBinary : Node_Base<Node_MathBinary>
    {
        public override void PostDeserialise()
        {
            value = operation switch
            {
                MathOperation.Add => value1 + value2,
                MathOperation.Subtract => value1 - value2,
                MathOperation.Multiply => value1 * value2,
                MathOperation.Divide => value1 / value2,
                _ => value1
            };
        }

        [Variable<MathOperation>]
        public MathOperation operation;

        [Input]
        public MathConvertible value1 = new();

        [Input]
        public MathConvertible value2 = new();

        [Output]
        public MathConvertible value;

        public enum MathOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }
    }
}
