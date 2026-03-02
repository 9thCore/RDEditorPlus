using UnityEngine;

namespace RDEditorPlus.Functionality.NodeEditor
{
    public class MergerPanelHolder : NodePanelHolder
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

        protected MergerPanelHolder()
        {
            title.text = "Level Merger Utility";
            title.color = Color.yellow;
        }
    }
}
