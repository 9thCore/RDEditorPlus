using RDEditorPlus.Functionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeDefinitions.Types
{
    public class Node_Test : Node_Base<Node_Test>
    {
        public override void PostDeserialise()
        {
            result = param + 3f;
            Plugin.LogInfo($"(variable = {variable}, param = {param}, result = {result})");
        }

        [Variable<float>(1.0f)]
        public float variable = 1.0f;

        [Input("in")]
        public float param;

        [Output("out")]
        public float result;
    }
}
