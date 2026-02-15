using RDLevelEditor;
using System;
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

            return CheckEqual(ev => propertyControl.GetEventValue(ev)?.ToString());
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_Checkbox propertyControl)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }

            return CheckEqual(ev => (bool)propertyControl.GetEventValue(ev));
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_ExpPositionPicker propertyControl, Component component)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }

            return component switch
            {
                Component.X => CheckEqual(ev => ((FloatExpression2)propertyControl.GetEventValue(ev)).x),
                Component.Y => CheckEqual(ev => ((FloatExpression2)propertyControl.GetEventValue(ev)).y),
                _ => false
            };
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_PositionPicker propertyControl, Component component)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }

            return component switch
            {
                Component.X => CheckEqual(ev => ((Float2)propertyControl.GetEventValue(ev)).X()),
                Component.Y => CheckEqual(ev => ((Float2)propertyControl.GetEventValue(ev)).Y()),
                _ => false
            };
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_Color propertyControl)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }

            ColorOrPalette value = (ColorOrPalette)propertyControl.GetEventValue(scnEditor.instance.selectedControls[0].levelEvent);

            return scnEditor.instance.selectedControls.Select(control => (ColorOrPalette)propertyControl.GetEventValue(control.levelEvent))
                .All(value2 => value2.paletteIndex == value.paletteIndex && value2.color == value.color);
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_Image propertyControl)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }

            string[] values = propertyControl.GetEventValue(scnEditor.instance.selectedControls[0].levelEvent).CastToStringArray();

            if (values == null)
            {
                return scnEditor.instance.selectedControls.All(array => array == null);
            }

            return scnEditor.instance.selectedControls.Select(control => propertyControl.GetEventValue(control.levelEvent).CastToStringArray())
                .All(value2 => value2 != null && values.SequenceEqual(value2));
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_SetRoomPerspective propertyControl, int index, Component component)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }

            return component switch
            {
                Component.X => CheckEqual(ev => ((Float2[])propertyControl.GetEventValue(ev))[index].X()),
                Component.Y => CheckEqual(ev => ((Float2[])propertyControl.GetEventValue(ev))[index].Y()),
                _ => false
            };
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_ShowDialogue propertyControl)
        {
            if (!InspectorUtil.CanMultiEdit())
            {
                return true;
            }
            return CheckEqual(ev => (string)propertyControl.GetEventValue(ev));
        }

        public static bool EqualValueForSelectedEvents(this PropertyControl_CharacterPicker propertyControl)
        {
            return CheckEqual(ev => (string)propertyControl.GetEventValue(ev));
        }

        public static bool EqualCharacterForSelectedEvents()
        {
            return CheckEqual(GetCharacter);
        }

        public enum Component
        {
            X,
            Y
        }

        private static bool CheckEqual<T>(Func<LevelEvent_Base, T> getter)
        {
            T value = getter(scnEditor.instance.selectedControls[0].levelEvent);

            if (value == null)
            {
                return scnEditor.instance.selectedControls.All(control => getter(control.levelEvent) == null);
            }

            return scnEditor.instance.selectedControls.All(control => value.Equals(getter(control.levelEvent)));
        }

        private static CharacterOrCustom GetCharacter(LevelEvent_Base levelEvent) => GetCharacter(levelEvent.row);
        private static CharacterOrCustom GetCharacter(int row)
        {
            var rows = scnEditor.instance.rowsData;
            if (row < 0 || row >= rows.Count)
            {
                return new(Character.None);
            }

            return rows[row].character == Character.Custom ? new(rows[row].customCharacterName) : new(rows[row].character);
        }

        private static string[] CastToStringArray(this object value)
        {
            if (value is not object[] array)
            {
                return null;
            }

            return Array.ConvertAll(array, Converter);
        }

        private static readonly Converter<object, string> Converter = new(Convert.ToString);

        private readonly struct CharacterOrCustom
        {
            public readonly Character character;
            public readonly string customCharacter;

            public bool IsCustom => customCharacter != null;

            public CharacterOrCustom(Character character)
            {
                this.character = character;
                customCharacter = null;
            }

            public CharacterOrCustom(string customCharacter)
            {
                character = default;
                this.customCharacter = customCharacter;
            }

            public override bool Equals(object obj)
            {
                if (obj is CharacterOrCustom that)
                {
                    if (IsCustom != that.IsCustom)
                    {
                        return false;
                    }

                    return IsCustom ? customCharacter == that.customCharacter : character == that.character;
                }

                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return (int)character;
            }
        }
    }
}
