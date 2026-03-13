using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Events
{
    public class Node_BarFilter : Node_Base<Node_BarFilter>
    {
        public override void PostDeserialise()
        {
            result = events.WithBarFilter(start, end);
        }

        [Variable<int>]
        public int start;

        [Variable<int>]
        public int end;

        [Input]
        public RDLevelEvents events;

        [Output("events")]
        public RDLevelEvents result;
    }
}
