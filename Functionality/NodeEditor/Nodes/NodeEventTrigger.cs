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

        public override void OnBeginDrag(PointerEventData eventData)
        {
            lastDragPosition = eventData.position;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            node.Drag(eventData.position - lastDragPosition);
            lastDragPosition = eventData.position;
        }

        private Vector2 lastDragPosition;

        [SerializeField]
        private Node node;
    }
}
