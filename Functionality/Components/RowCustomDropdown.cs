using RDEditorPlus.ExtraData;
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
    }
}
