using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Util;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public class Float2NodeVariable : NodeVariable<Float2NodeVariable, Float2>
    {
        public override object Value
            => (currentValue.xUsed ? currentValue.x.ToString() : string.Empty) +
            Float2Util.SerialisationSeparator +
            (currentValue.yUsed ? currentValue.y.ToString() : string.Empty);

        public override bool CanSave()
            => currentValue.x != initialValue.x || currentValue.xUsed != initialValue.xUsed
            || currentValue.y != initialValue.y || currentValue.yUsed != initialValue.yUsed;

        protected override void OnVariableChange(string text)
        {
            currentValue = Float2Util.ParseSerialised(text);
            base.OnVariableChange(text);
        }

        protected override void SetRepresentation(string value)
        {
            inputField.SetTextWithoutNotify(currentValue.xUsed ? currentValue.x.ToString() : string.Empty);
            yInputField.SetTextWithoutNotify(currentValue.yUsed ? currentValue.y.ToString() : string.Empty);
        }

        private void OnXVariableChange(string text)
        {
            if (float.TryParse(text, out var result))
            {
                currentValue.x = result;
                currentValue.xUsed = true;
            }
            else
            {
                currentValue.xUsed = false;
            }

            base.OnVariableChange(text);
        }

        private void OnYVariableChange(string text)
        {
            if (float.TryParse(text, out var result))
            {
                currentValue.y = result;
                currentValue.yUsed = true;
            }
            else
            {
                currentValue.yUsed = false;
            }

            base.OnVariableChange(text);
        }

        private void Awake()
        {
            ((Text)inputField.placeholder).text = "--";
            ((Text)yInputField.placeholder).text = "--";

            inputField.onEndEdit.AddListener(OnXVariableChange);
            yInputField.onEndEdit.AddListener(OnYVariableChange);
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
                    variablePrefab.name += "Float2";

                    var variable = variablePrefab.GetComponent<Float2NodeVariable>();
                    variable.type = Node.Type.Float2;

                    var xInputField = variable.inputField;
                    xInputField.characterValidation = InputField.CharacterValidation.Decimal;
                    xInputField.contentType = InputField.ContentType.DecimalNumber;
                    xInputField.lineType = InputField.LineType.SingleLine;
                    xInputField.characterLimit = 7;

                    AddInputField(variablePrefab.transform, out var yInputFieldRT, out var yInputField);
                    AddPlaceholder(yInputField);

                    variable.yInputField = yInputField;
                    yInputField.characterValidation = InputField.CharacterValidation.Decimal;
                    yInputField.contentType = InputField.ContentType.DecimalNumber;
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
