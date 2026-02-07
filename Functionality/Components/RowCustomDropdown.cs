using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine.EventSystems;

namespace RDEditorPlus.Functionality.Components
{
    public class RowCustomDropdown : CustomDropdown
    {
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            PropertyStorage.Instance.rowChanged = true;
        }

        protected override bool IsOfEqualValue => !this.TryGetComponentInParent(out InspectorPanel panel) || panel.RowEqualValueForSelectedEvents();
    }
}
