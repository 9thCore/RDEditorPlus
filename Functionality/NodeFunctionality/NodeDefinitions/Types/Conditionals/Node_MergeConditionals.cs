using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using System;
using System.Linq;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Conditionals
{
    public class Node_MergeConditionals : Node_Base<Node_MergeConditionals>
    {
        public override void PostDeserialise()
        {
            var list1 = conditionals1.Collect();

            // Conditionals are 1-indexed
            // This might leave gaps in the indices, but filling them is a pain in the ass
            // (and this mod isn't intended for optimising levels, anyway)
            int offset = list1.Max(conditional => conditional.id);

            var list2 = conditionals2.Collect();
            foreach (var conditional in list2)
            {
                // Auto-generated tags are the id as a string, so replace it too if needed
                if (int.TryParse(conditional.tag, out var result) && result == conditional.id)
                {
                    conditional.tag = (conditional.id + offset).ToString();
                }

                conditional.id += offset;
            }
            list1.AddRange(list2);

            conditionals = list1;
            events = events2.WithConditionalOffset(offset);
        }

        [Input]
        public RDLevelConditionals conditionals1;

        [Input]
        public RDLevelConditionals conditionals2;

        [Input]
        public RDLevelEvents events2;

        [Output]
        public RDLevelConditionals conditionals;

        [Output]
        public RDLevelEvents events;
    }
}
