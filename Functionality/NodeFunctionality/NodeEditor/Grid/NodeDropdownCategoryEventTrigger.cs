using RDLevelEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid
{
    public class NodeDropdownCategoryEventTrigger : RDEventTrigger
    {
        public void Setup(NodeDropdown dropdown, GameObject parent, GameObject target)
        {
            this.dropdown = dropdown;
            this.parent = parent;
            this.target = target;
        }

        public override void OnPointerClick(PointerEventData _)
        {
            parent.SetActive(false);
            target.SetActive(true);

            target.transform.position = dropdown.transform.position;
            target.transform.SetAsLastSibling();
            dropdown.CurrentSubtree = target;
        }

        private NodeDropdown dropdown;
        private GameObject parent;
        private GameObject target;
    }
}
