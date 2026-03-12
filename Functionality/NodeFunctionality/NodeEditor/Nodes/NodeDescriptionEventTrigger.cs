using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes
{
    public class NodeDescriptionEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        public void Setup(string description)
        {
            if (description.IsNullOrEmpty())
            {
                GameObject.Destroy(this);
                return;
            }

            gameObject.AddComponent<GraphicRaycaster>();
            this.description = description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            NodeGridView.ShowDescription(description);
            NodeGridView.MoveDescription(eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            NodeGridView.HideDescription();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            NodeGridView.MoveDescription(eventData.position);
        }

        [SerializeField]
        private string description;
    }
}
