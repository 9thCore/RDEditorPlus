using RDLevelEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid
{
    public class NodeGridEventTrigger : RDEventTrigger
    {
        public void Setup(NodeGrid grid)
        {
            this.grid = grid;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                NodeDropdown.Instance.Activate(eventData.position, grid);
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                lastDragPosition = eventData.position;
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                grid.Drag(eventData.position - lastDragPosition);
                lastDragPosition = eventData.position;
            }
        }

        private Vector2 lastDragPosition;
        private NodeGrid grid;
    }
}
