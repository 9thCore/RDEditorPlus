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
            object value = propertyControl.GetEventValue(scnEditor.instance.selectedControls[0].levelEvent);
            string valueString = value?.ToString();

            return scnEditor.instance.selectedControls.All(
                eventControl => propertyControl.GetEventValue(eventControl.levelEvent)?.ToString() == valueString);
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_ExpPositionPicker propertyControl, out bool xEqualBetweenEvents, out bool yEqualBetweenEvents)
        {
            xEqualBetweenEvents = true;
            yEqualBetweenEvents = true;

            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }

            FloatExpression2 expression = (FloatExpression2)propertyControl.GetEventValue(scnEditor.instance.selectedControls[0].levelEvent);

            for (int i = 1; i < scnEditor.instance.selectedControls.Count; i++)
            {
                LevelEventControl_Base eventControl = scnEditor.instance.selectedControls[i];

                FloatExpression2 expression2 = (FloatExpression2)propertyControl.GetEventValue(eventControl.levelEvent);

                if (!expression.x.Equal(expression2.x))
                {
                    xEqualBetweenEvents = false;
                }

                if (!expression.y.Equal(expression2.y))
                {
                    yEqualBetweenEvents = false;
                }

                if (!xEqualBetweenEvents && !yEqualBetweenEvents)
                {
                    return false;
                }
            }

            return xEqualBetweenEvents && yEqualBetweenEvents;
        }
    }
}
