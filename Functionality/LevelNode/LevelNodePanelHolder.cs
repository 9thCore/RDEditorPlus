using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor;
using UnityEngine;

namespace RDEditorPlus.Functionality.LevelNode
{
    public class LevelNodePanelHolder : NodePanelHolder<LevelNodeDataRoot>
    {
        private static LevelNodePanelHolder instance;
        public static LevelNodePanelHolder Instance
        {
            get
            {
                if (instance == null || !instance.Valid())
                {
                    instance = new();
                }

                return instance;
            }
        }

        public override string DefaultFilename => "level";
        public override string[] Extensions => ["rdlevelnode"];

        protected LevelNodePanelHolder()
        {
            title.text = "Level Node Utility";
            title.color = Color.yellow;

            CloneButton(rectTransform, "Execute", Simulate,
                anchorMin: new Vector2(0.51f, 0.01f), anchorMax: new Vector2(0.98f, 0.08f));
        }
    }
}
