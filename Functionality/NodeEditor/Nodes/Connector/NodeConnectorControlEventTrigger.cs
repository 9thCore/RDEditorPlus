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
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                connector.StartConnection();
                connector.UpdateConnection(eventData.position);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                connector.UpdateConnection(eventData.position);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                connector.EndConnection(eventData.position);
            }
        }

        [SerializeField]
        private NodeConnector connector;
    }
}
