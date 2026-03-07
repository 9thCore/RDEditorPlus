using RDLevelEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes
{
    public class NodeEventTrigger : RDEventTrigger
    {
        public void Setup(Node node)
        {
            this.node = node;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                node.Delete(dontDeleteFromGrid: false);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                transform.SetAsLastSibling();
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
                node.Drag(eventData.position - lastDragPosition);
                lastDragPosition = eventData.position;
            }
        }

        private Vector2 lastDragPosition;

        [SerializeField]
        private Node node;
    }
}
