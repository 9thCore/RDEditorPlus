using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Util;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public class FloatExpression2NodeVariable : NodeVariable<FloatExpression2NodeVariable, FloatExpression2>
    {
        public override object Value => FloatExpression2Util.Serialise(currentValue);

        public override bool CanSave() => !currentValue.x.Equal(initialValue.x) || !currentValue.y.Equal(initialValue.y);

        protected override void OnVariableChange(string text)
        {
            currentValue = FloatExpression2Util.ParseSerialised(text);
            base.OnVariableChange(text);
        }

        protected override void SetRepresentation(string value)
        {
            inputField.SetTextWithoutNotify(currentValue.x.ToString());
            yInputField.SetTextWithoutNotify(currentValue.y.ToString());
        }

        private void OnXVariableChange(string text)
        {
            if (float.TryParse(text, out var value))
            {
                currentValue = new(new FloatExpression(value), currentValue.y);
            }
            else
            {
                currentValue = new(new FloatExpression(text), currentValue.y);
            }

            base.OnVariableChange(text);
        }

        private void OnYVariableChange(string text)
        {
            if (float.TryParse(text, out var value))
            {
                currentValue = new(currentValue.x, new FloatExpression(value));
            }
            else
            {
                currentValue = new(currentValue.x, new FloatExpression(text));
            }

            base.OnVariableChange(text);
        }

        private void Awake()
        {
            ((Text)inputField.placeholder).text = "--";
            ((Text)yInputField.placeholder).text = "--";

            inputField.onEndEdit.AddListener(OnXVariableChange);
            yInputField.onEndEdit.AddListener(OnYVariableChange);

            // hardcoding because idk why its set to (x: 0, y: 0) otherwise
            currentValue = initialValue = new(FloatExpression.EmptyInput(), FloatExpression.EmptyInput());
        }

        [SerializeField]
        private InputField yInputField;

        public static GameObject VariablePrefab
        {
            get
            {
                if (variablePrefab == null)
                {
                    variablePrefab = Instantiate(InputFieldTextPlaceholderVariable);
                    variablePrefab.name += "FloatExpression2";

                    var variable = variablePrefab.GetComponent<FloatExpression2NodeVariable>();
                    variable.type = Node.Type.FloatExpression2;

                    var xInputField = variable.inputField;
                    xInputField.characterValidation = InputField.CharacterValidation.None;
                    xInputField.contentType = InputField.ContentType.Standard;
                    xInputField.lineType = InputField.LineType.SingleLine;
                    xInputField.characterLimit = 7;

                    AddInputField(variablePrefab.transform, out var yInputFieldRT, out var yInputField);
                    AddPlaceholder(yInputField);

                    variable.yInputField = yInputField;
                    yInputField.characterValidation = InputField.CharacterValidation.None;
                    yInputField.contentType = InputField.ContentType.Standard;
                    yInputField.lineType = InputField.LineType.SingleLine;
                    yInputField.characterLimit = 7;

                    var xInputFieldRT = xInputField.transform as RectTransform;

                    xInputFieldRT.anchorMin = new Vector2(0.5f, 0.5f);
                    yInputFieldRT.anchorMax = new Vector2(1f, 0.5f);
                    xInputFieldRT.offsetMin = new Vector2(2f, 1f);
                    yInputFieldRT.offsetMax = new Vector2(-2f, -1f);

                    variablePrefab.GetComponent<TextLayoutElementPropagator>().SetHeightPadding(16f);
                }

                return variablePrefab;
            }
        }

        private static GameObject variablePrefab;
    }
}
