using RDEditorPlus.Functionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeDefinitions.Types
{
    public class Node_Test : Node_Base<Node_Test>
    {
        public override void PostDeserialise()
        {
            result = param + 3f;
            resultInt = paramInt + integer;
            Plugin.LogInfo($"(variable = {variable}, integer = {integer})");
            Plugin.LogInfo($"(param = {param}, result = {result})");
            Plugin.LogInfo($"(paramInt = {paramInt}, resultInt = {resultInt})");
        }

        [Variable<float>(1.0f)]
        public float variable = 1.0f;

        [Variable<int>]
        public int integer;

        [Input("in")]
        public float param;

        [Input]
        public int paramInt;

        [Output]
        public int resultInt;

        [Output("out")]
        public float result;
    }
}
