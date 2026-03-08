namespace RDEditorPlus.Functionality.NodeDefinitions.Types
{
    public class Node_Test : Node_Base<Node_Test>
    {
        public override void PostDeserialise()
        {
            result = param + 3f;
            Plugin.LogInfo($"(param = {param}, result = {result})");
        }

        [Input("in")]
        public float param;

        [Output("out")]
        public float result;
    }
}
