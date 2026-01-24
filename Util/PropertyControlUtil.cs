using RDLevelEditor;
using System.Linq;

namespace RDEditorPlus.Util
{
    internal static class PropertyControlUtil
    {
        public static bool EqualValueForSelectedEvents(this PropertyControl propertyControl)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }

            //object value = propertyControl.GetEventValue(scnEditor.instance.selectedControls[0].levelEvent);
            //return scnEditor.instance.selectedControls.All(eventControl => propertyControl.GetEventValue(eventControl.levelEvent) == value);

            // I'm aware stringifying everything isn't too great, but I'll swap this out later if needed
            string value = propertyControl.GetEventValue(scnEditor.instance.selectedControls[0].levelEvent).ToString();
            return scnEditor.instance.selectedControls.All(eventControl => propertyControl.GetEventValue(eventControl.levelEvent).ToString() == value);
        }
    }
}
