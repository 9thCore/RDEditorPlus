using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public class FloatNodeVariable : NodeVariable<FloatNodeVariable, float>
    {
        protected override void OnVariableChange(string text)
        {
            if (float.TryParse(text, out var result))
            {
                currentValue = result;
            }
            else
            {
                currentValue = initialValue;
            }

            base.OnVariableChange(text);
        }

        protected override void SetInitialValue(object initialValue)
        {
            var initialValueString = initialValue.ToString();
            ((Text)inputField.placeholder).text = initialValueString;

            base.SetInitialValue(initialValue);
        }

        protected override void SetRepresentation(string value)
        {
            inputField.SetTextWithoutNotify(value);
        }

        public static GameObject VariablePrefab
        {
            get
            {
                if (variablePrefab == null)
                {
                    variablePrefab = Instantiate(InputFieldTextPlaceholderVariable);
                    variablePrefab.name += "Float";

                    var variable = variablePrefab.GetComponent<FloatNodeVariable>();
                    variable.type = Node.Type.Single;

                    var inputField = variable.inputField;
                    inputField.characterValidation = InputField.CharacterValidation.Decimal;
                    inputField.contentType = InputField.ContentType.DecimalNumber;
                    inputField.lineType = InputField.LineType.SingleLine;
                    inputField.characterLimit = 7;
                }

                return variablePrefab;
            }
        }

        private static GameObject variablePrefab;
    }
}
