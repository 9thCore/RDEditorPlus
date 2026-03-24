using RDLevelEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes
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
                startDragPosition = node.transform.position;
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

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                node.Drag(eventData.position - lastDragPosition);
                node.SendDragEvent(startDragPosition);
            }
        }

        private Vector2 lastDragPosition;
        private Vector2 startDragPosition;

        [SerializeField]
        private Node node;
    }
}
