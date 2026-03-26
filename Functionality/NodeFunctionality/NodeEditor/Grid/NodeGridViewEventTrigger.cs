using RDLevelEditor;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid
{
    internal class NodeGridViewEventTrigger : RDEventTrigger
    {
        public void Setup(NodeGrid grid)
        {
            this.grid = grid;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            grid.CanZoom = true;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            grid.CanZoom = false;
        }

        private NodeGrid grid;
    }
}
