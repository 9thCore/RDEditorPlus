using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public class FloatExpressionNodeVariable : NodeVariable<FloatExpressionNodeVariable, FloatExpression>
    {
        public override bool CanSave() => CurrentValue != initialValue;

        protected override void OnVariableChange(string text)
        {
            if (float.TryParse(text, out var result))
            {
                CurrentValue = new FloatExpression(result);
            }
            else
            {
                CurrentValue = new FloatExpression(text);
            }

            base.OnVariableChange(text);
        }

        protected override void SetInitialValue(object initialValue)
        {
            ((Text)inputField.placeholder).text = "--";

            base.SetInitialValue(initialValue);
        }

        protected override void SetRepresentation(string value)
        {
            inputField.SetTextWithoutNotify(value);
        }

        private void Start()
        {
            // (commented out because I'll fix it later when math is added back)
            // CurrentValue = initialValue = FloatExpression.EmptyInput();
        }

        public static GameObject VariablePrefab
        {
            get
            {
                if (variablePrefab == null)
                {
                    variablePrefab = Instantiate(InputFieldTextPlaceholderVariable);
                    variablePrefab.name += "FloatExpression";

                    var variable = variablePrefab.GetComponent<FloatExpressionNodeVariable>();
                    variable.type = Node.Type.Single;

                    var inputField = variable.inputField;
                    inputField.characterValidation = InputField.CharacterValidation.None;
                    inputField.contentType = InputField.ContentType.Standard;
                    inputField.lineType = InputField.LineType.SingleLine;
                    inputField.characterLimit = 7;
                }

                return variablePrefab;
            }
        }

        private static GameObject variablePrefab;
    }
}
