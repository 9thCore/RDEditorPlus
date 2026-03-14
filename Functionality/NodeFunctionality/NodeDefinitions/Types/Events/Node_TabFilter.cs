using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Events
{
    public class Node_TabFilter : Node_Base<Node_TabFilter>
    {
        public override void PostDeserialise()
        {
            output = input.WithTabFilter(sounds, rows, actions, sprites, rooms, windows);
        }

        [Variable<bool>(true)]
        public bool sounds = true;

        [Variable<bool>(true)]
        public bool rows = true;

        [Variable<bool>(true)]
        public bool actions = true;

        [Variable<bool>(true)]
        public bool rooms = true;

        [Variable<bool>(true)]
        public bool sprites = true;

        [Variable<bool>(true)]
        public bool windows = true;

        [Input("events")]
        public RDLevelEvents input;

        [Output("events")]
        public RDLevelEvents output;
    }
}
