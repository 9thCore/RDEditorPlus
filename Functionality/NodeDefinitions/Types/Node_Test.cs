namespace RDEditorPlus.Functionality.NodeDefinitions.Types
{
    public class Node_Test : Node_Base<Node_Test.InputStorage, Node_Test.OutputStorage>
    {
        public override void PostDeserialise()
        {
            output.result = input.param + 3f;
            Plugin.LogInfo($"Result = {output.result}");
        }

        public class InputStorage
        {
            public float param;
        }

        public class OutputStorage
        {
            public float result;
        }
    }
}
