using RDLevelEditor;
using System.Linq;

namespace RDEditorPlus.Util
{
    internal static class PropertyControlUtil
    {
        public static bool EqualValueForSelectedEvents(this PropertyControl propertyControl, LevelEvent_Base levelEvent)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return false;
            }

            object value = propertyControl.GetEventValue(levelEvent);
            return scnEditor.instance.selectedControls.All(eventControl => propertyControl.GetEventValue(eventControl.levelEvent) != value);
        }
    }
}
