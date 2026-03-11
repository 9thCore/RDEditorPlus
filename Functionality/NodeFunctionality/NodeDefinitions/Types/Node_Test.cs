using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types
{
    public class Node_Test : Node_Base<Node_Test>
    {
        public override void PostDeserialise()
        {
            result = param + 3f;
            Plugin.LogInfo($"(level = {level})");
            Plugin.LogInfo($"(param = {param}, result = {result})");
        }

        [Variable]
        public RDLevelFile level;

        [Input("in")]
        public float param;

        [Output("out")]
        public float result;
    }
}
