
using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Sprites
{
    public class Node_EnsureUniqueSprites : Node_Base<Node_EnsureUniqueSprites>
    {
        public override void PostDeserialise()
        {
            int spritePostfix = simulator.SpritePostfix;

            outputEvents = inputEvents.WithUniqueSpriteIDs(spritePostfix);

            var list = inputSprites.Collect();
            foreach (var sprite in list)
            {
                sprite.spriteId = $"{sprite.spriteId}{spritePostfix}";
            }
            outputSprites = list;
        }

        [Input("sprites")]
        public RDLevelSprites inputSprites;

        [Input("events")]
        public RDLevelEvents inputEvents;

        [Output("sprites")]
        public RDLevelSprites outputSprites;

        [Output("events")]
        public RDLevelEvents outputEvents;
    }
}
