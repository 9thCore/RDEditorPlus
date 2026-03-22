using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using System.Linq;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Sprites
{
    public class Node_MergeSprites : Node_Base<Node_MergeSprites>
    {
        public override void PostDeserialise()
        {
            var list = input1.Collect();

            var list2 = input2.Collect();
            foreach (var sprite in list2)
            {
                if (list.Any(spr => spr.spriteId == sprite.spriteId))
                {
                    Plugin.LogInfo($"Sprite of same ID '{sprite.spriteId}' found in both inputs, discarding second instance.\n(Make the sprite IDs unique with the Sprites/Ensure Unique node if this is not desired!)");
                }
                else
                {
                    list.Add(sprite);
                }
            }

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
