using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public class StringNodeVariable : NodeVariable<StringNodeVariable, string>
    {
        protected override void OnVariableChange(string text)
        {
            if (text.IsNullOrEmpty())
            {
                currentValue = initialValue;
            }
            else
            {
                currentValue = text;
            }

            base.OnVariableChange(text);
        }

        protected override void SetInitialValue(object initialValue)
        {
            ((Text)inputField.placeholder).text = initialValue.ToString();
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
                    variablePrefab.name += "String";

                    var variable = variablePrefab.GetComponent<StringNodeVariable>();
                    variable.type = Node.Type.String;

                    var inputField = variable.inputField;
                    inputField.characterValidation = InputField.CharacterValidation.None;
                    inputField.contentType = InputField.ContentType.Standard;
                    inputField.lineType = InputField.LineType.SingleLine;
                }

                return variablePrefab;
            }
        }

        private static GameObject variablePrefab;
    }
}
