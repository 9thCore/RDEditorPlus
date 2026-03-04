using RDLevelEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes.Connector
{
    public class NodeConnectorControlEventTrigger : RDEventTrigger
    {
        public void Setup(NodeConnector connector)
        {
            this.connector = connector;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            connector.StartConnection();
            connector.UpdateConnection(eventData.position);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            connector.UpdateConnection(eventData.position);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            connector.EndConnection(eventData.position);
        }

        [SerializeField]
        private NodeConnector connector;
    }
}
