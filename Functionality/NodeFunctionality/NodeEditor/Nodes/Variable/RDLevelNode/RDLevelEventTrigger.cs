using UnityEngine;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable.RDLevelNode
{
    public class RDLevelEventTrigger : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public RDLevelEventTrigger Setup(RDLevelNodeVariable variable)
        {
            this.variable = variable;
            return this;
        }

        public void OnPointerClick(PointerEventData _)
        {
            variable.SelectLevel();
        }

        public void OnPointerDown(PointerEventData _) { }
        public void OnPointerUp(PointerEventData _) { }

        [SerializeField]
        private RDLevelNodeVariable variable;
    }
}
