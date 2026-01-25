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

            object value = propertyControl.GetEventValue(scnEditor.instance.selectedControls[0].levelEvent);
            string valueXStringified = null;
            string valueYStringified = null;
            bool flag = value != null;

            if (flag)
            {
                FloatExpression2 expression = (FloatExpression2) value;
                valueXStringified = expression.x.ToString();
                valueYStringified = expression.y.ToString();
            }

            for (int i = 1; i < scnEditor.instance.selectedControls.Count; i++)
            {
                LevelEventControl_Base eventControl = scnEditor.instance.selectedControls[i];

                object value2 = propertyControl.GetEventValue(eventControl.levelEvent);
                bool flag2 = value2 != null;

                if (flag != flag2)
                {
                    xEqualBetweenEvents = false;
                    yEqualBetweenEvents = false;
                    return false;
                }

                if (flag && flag2)
                {
                    FloatExpression2 expression2 = (FloatExpression2) value2;
                    if (valueXStringified != expression2.x.ToString())
                    {
                        xEqualBetweenEvents = false;
                    }

                    if (valueYStringified != expression2.y.ToString())
                    {
                        yEqualBetweenEvents = false;
                    }

                    if (!xEqualBetweenEvents && !yEqualBetweenEvents)
                    {
                        return false;
                    }
                }
            }

            return xEqualBetweenEvents && yEqualBetweenEvents;
        }
    }
}
