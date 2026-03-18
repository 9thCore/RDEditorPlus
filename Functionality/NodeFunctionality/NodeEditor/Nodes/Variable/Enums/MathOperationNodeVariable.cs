using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Maths;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable.Enums
{
    public class MathOperationNodeVariable : EnumNodeVariable<MathOperationNodeVariable, Node_MathBinary.MathOperation>
    {
        public override bool CanSave() => currentValue != initialValue;

        public static GameObject VariablePrefab => GetCachedPrefab(Node.Type.MathOperation);
    }
}
