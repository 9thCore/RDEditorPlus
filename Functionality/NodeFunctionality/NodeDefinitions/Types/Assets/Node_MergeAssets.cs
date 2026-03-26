using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using System.Linq;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Assets
{
    public class Node_MergeAssets : Node_Base<Node_MergeAssets>
    {
        public override void PostDeserialise()
        {
            var list = input1.Collect();

            var list2 = input2.Collect();
            foreach (var asset in list2)
            {
                if (list.Any(ass => ass.SameFinalAssetAs(asset)))
                {
                    Plugin.LogInfo($"Asset of same final location after merging '{asset.Filename}' found in both inputs, discarding second instance.");
                }
                else
                {
                    list.Add(asset);
                }
            }

            output = list;
        }

        [Input]
        public RDLevelAssets input1;

        [Input]
        public RDLevelAssets input2;

        [Output]
        public RDLevelAssets output;
    }
}
