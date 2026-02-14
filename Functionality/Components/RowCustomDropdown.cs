using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;

namespace RDEditorPlus.Functionality.Components
{
    public class RowCustomDropdown : CustomDropdown
    {
        protected override void OnSelect()
        {
            PropertyStorage.Instance.rowChanged = true;
        }

        protected override bool IsOfEqualValue => !this.TryGetComponentInParent(out InspectorPanel panel) || panel.RowEqualValueForSelectedEvents();
    }
}
