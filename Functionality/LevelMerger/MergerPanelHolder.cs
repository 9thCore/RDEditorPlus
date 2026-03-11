using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor;
using UnityEngine;

namespace RDEditorPlus.Functionality.LevelMerger
{
    public class MergerPanelHolder : NodePanelHolder<MergeNodeDataRoot>
    {
        private static MergerPanelHolder instance;
        public static MergerPanelHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new();
                }

                return instance;
            }
        }

        public override string DefaultFilename => "level";
        public override string[] Extensions => ["rdmerge"];

        protected MergerPanelHolder()
        {
            title.text = "Level Merger Utility";
            title.color = Color.yellow;

            CloneButton(rectTransform, "Export", Simulate,
                anchorMin: new Vector2(0.51f, 0.01f), anchorMax: new Vector2(0.98f, 0.08f));
        }
    }
}
