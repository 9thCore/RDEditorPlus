using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public class IntegerNodeVariable : NodeVariable<IntegerNodeVariable, int>
    {
        public override bool CanSave() => CurrentValue != initialValue;

        protected override void OnVariableChange(string text)
        {
            if (int.TryParse(text, out var result))
            {
                CurrentValue = result;
            }
            else
            {
                CurrentValue = initialValue;
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
                    variablePrefab.name += "Integer";

                    var variable = variablePrefab.GetComponent<IntegerNodeVariable>();
                    variable.type = Node.Type.Int32;

                    var inputField = variable.inputField;
                    inputField.characterValidation = InputField.CharacterValidation.Integer;
                    inputField.contentType = InputField.ContentType.IntegerNumber;
                    inputField.lineType = InputField.LineType.SingleLine;
                    inputField.characterLimit = 7;
                }

                return variablePrefab;
            }
        }

        private static GameObject variablePrefab;
    }
}
