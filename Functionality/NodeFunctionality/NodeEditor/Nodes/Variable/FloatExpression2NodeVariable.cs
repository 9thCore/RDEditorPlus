using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Util;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public class FloatExpression2NodeVariable : NodeVariable<FloatExpression2NodeVariable, FloatExpression2>
    {
        public override object Value => FloatExpression2Util.Serialise(CurrentValue);

        public override bool CanSave() => !CurrentValue.x.Equal(initialValue.x) || !CurrentValue.y.Equal(initialValue.y);

        protected override void OnVariableChange(string text)
        {
            CurrentValue = FloatExpression2Util.ParseSerialised(text);
            base.OnVariableChange(text);
        }

        protected override void SetRepresentation(string value)
        {
            inputField.SetTextWithoutNotify(CurrentValue.x.ToString());
            yInputField.SetTextWithoutNotify(CurrentValue.y.ToString());
        }

        private void OnXVariableChange(string text)
        {
            if (float.TryParse(text, out var value))
            {
                CurrentValue = new(new FloatExpression(value), CurrentValue.y);
            }
            else
            {
                CurrentValue = new(new FloatExpression(text), CurrentValue.y);
            }

            base.OnVariableChange(text);
        }

        private void OnYVariableChange(string text)
        {
            if (float.TryParse(text, out var value))
            {
                CurrentValue = new(CurrentValue.x, new FloatExpression(value));
            }
            else
            {
                CurrentValue = new(CurrentValue.x, new FloatExpression(text));
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
            // (commented out because I'll fix it later when math is added back)
            // CurrentValue = initialValue = new(FloatExpression.EmptyInput(), FloatExpression.EmptyInput());
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
