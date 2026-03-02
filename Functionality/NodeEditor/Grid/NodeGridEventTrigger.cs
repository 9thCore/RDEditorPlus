using RDLevelEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.NodeEditor.Grid
{
    public class NodeGridEventTrigger : RDEventTrigger
    {
        public void Setup(NodeGrid grid)
        {
            this.grid = grid;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            lastDragPosition = eventData.position;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            grid.Drag(eventData.position - lastDragPosition);
            lastDragPosition = eventData.position;
        }

        private Vector2 lastDragPosition;
        private NodeGrid grid;
    }
}
