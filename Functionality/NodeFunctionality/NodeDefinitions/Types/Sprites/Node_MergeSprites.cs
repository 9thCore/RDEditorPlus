using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Sprites
{
    public class Node_MergeSprites : Node_Base<Node_MergeSprites>
    {
        public override void PostDeserialise()
        {
            var list = input1.Collect();
            list.AddRange(input2.Collect());
            output = list;
        }

        [Input]
        public RDLevelSprites input1;

        [Input]
        public RDLevelSprites input2;

        [Output]
        public RDLevelSprites output;
    }
}
