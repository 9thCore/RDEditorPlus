namespace RDEditorPlus.Functionality.NodeDefinitions.Types
{
    public class Node_Test : Node_Base<Node_Test.InputStorage, Node_Test.OutputStorage>
    {
        public class InputStorage
        {
            public float param;

            public void Apply(params object[] parameters)
            {
                param = (float)parameters[0];
            }
        }

        public class OutputStorage
        {
            public float result;

            public void Apply(object[] results)
            {
                results[0] = result;
            }
        }
    }
}
