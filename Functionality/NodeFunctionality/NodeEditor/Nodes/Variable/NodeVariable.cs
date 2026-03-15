using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable.RDLevelNode;
using RDEditorPlus.Util;
using System;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public abstract class NodeVariable : MonoBehaviour, ISerializableNodeWorkspace.INode.IVariable
    {
        public string Name => variableName;
        public abstract object Value { get; }
        public abstract bool CanSave();

        public async Task SaveAsync(XmlWriter writer)
        {
            await writer.WriteAttributeStringAsync(NameKey, variableName);
            await writer.WriteElementStringAsync(ValueKey, Value.ToString());
        }

        public void SetValue(string value) => OnVariableChange(value);

        public readonly struct Data(Node.Type type, string name, object initialValue, string description)
        {
            public readonly void Setup(Node node)
            {
                var variable = Get(type, name, initialValue);
                if (variable != null)
                {
                    node.AddVariable(variable.rectTransform, variable, description);
                }
            }
        }

        protected abstract void SetRepresentation(string value);
        protected abstract void SetInitialValue(object initialValue);
        protected abstract void OnVariableChange(string text);

        private void SetName(string text)
        {
            this.text.text = text;
            variableName = text;
        }

        [SerializeField]
        protected InputField inputField;
        [SerializeField]
        protected string variableName;
        [SerializeField]
        protected Text text;
        [SerializeField]
        protected RectTransform rectTransform;
        [SerializeField]
        protected Node.Type type;

        public const string NameKey = "name";
        public const string ValueKey = "Value";

        private static NodeVariable Get(Node.Type type, string name, object initialValue)
        {
            var instance = Instantiate(Node.GetVariableType(type).Prefab);
            var variable = instance.GetComponent<NodeVariable>();
            variable.SetInitialValue(initialValue);
            variable.SetName(name);

            instance.SetActive(true);
            return variable;
        }
    }

    public abstract class NodeVariable<NodeVariableDerivative, VariableType> : NodeVariable
        where NodeVariableDerivative : NodeVariable<NodeVariableDerivative, VariableType>
        where VariableType : IEquatable<VariableType>
    {
        public override object Value => currentValue;
        public override bool CanSave() => !initialValue.Equals(currentValue);

        protected override void OnVariableChange(string text)
        {
            SetRepresentation(currentValue.ToString());
        }

        protected override void SetInitialValue(object initialValue)
        {
            if (initialValue is not VariableType cast)
            {
                return;
            }

            this.initialValue = cast;
            currentValue = cast;
            SetRepresentation(initialValue.ToString());
        }

        [SerializeField]
        protected VariableType initialValue;
        [SerializeField]
        protected VariableType currentValue;

        private void Awake()
        {
            if (inputField != null)
            {
                inputField.onEndEdit.AddListener(OnVariableChange);
            }
        }

        private const float NodeLayoutLeftPadding = 2f;
        private const float NodeLayoutRightPadding = 2f;
        private const float InputFieldLeftPadding = 4f;
        private const float InputFieldRightPadding = 4f;

        protected static GameObject AddBrowseButton(RectTransform inputField)
        {
            var sprite = AssetUtil.Browse1Sprite;
            float width = sprite.bounds.size.x;

            inputField.offsetMax -= new Vector2(width + 2f, 0f);

            GameObject browse = new("browse");

            var image = browse.AddComponent<Image>();
            image.sprite = sprite;

            var transform = browse.transform as RectTransform;
            transform.SetParent(inputField.parent);
            transform.localScale = Vector3.one;
            transform.anchorMin = new Vector2(1f, 0f);
            transform.anchorMax = Vector2.one;
            transform.offsetMin = new Vector2(-width, 0f);
            transform.offsetMax = Vector2.zero;
            transform.anchoredPosition -= new Vector2(2f, 0f);

            return browse;
        }

        protected static GameObject InputFieldTextPlaceholderVariable
        {
            get
            {
                if (inputFieldTextPlaceholderVariable == null)
                {
                    inputFieldTextPlaceholderVariable = Instantiate(InputFieldVariable);
                    var variable = inputFieldTextPlaceholderVariable.GetComponent<NodeVariableDerivative>();
                    var inputField = variable.inputField;

                    GameObject placeholder = new("placeholder");

                    var text = placeholder.AddComponent<Text>();
                    text.ApplyRDFont();
                    text.alignment = TextAnchor.LowerLeft;
                    text.color = Color.black.WithAlpha(0.33f);

                    var textRT = placeholder.transform as RectTransform;
                    textRT.SetParent(inputField.transform);
                    textRT.localScale = Vector3.one;
                    textRT.anchorMin = Vector2.zero;
                    textRT.anchorMax = Vector2.one;
                    textRT.offsetMin = new Vector2(InputFieldLeftPadding, 0f);
                    textRT.offsetMax = new Vector2(-InputFieldRightPadding, 0f);

                    inputField.placeholder = text;
                }

                return inputFieldTextPlaceholderVariable;
            }
        }

        protected static GameObject InputFieldVariable
        {
            get
            {
                if (inputFieldVariable == null)
                {
                    inputFieldVariable = Instantiate(BaseVariable);
                    var variable = inputFieldVariable.GetComponent<NodeVariableDerivative>();

                    GameObject inputField = new("inputField");

                    var image = inputField.AddComponent<Image>();
                    image.type = Image.Type.Tiled;
                    image.sprite = AssetUtil.InputFieldSprite;

                    var field = inputField.AddComponent<InputField>();
                    field.caretColor = Color.black;

                    var inputFieldRT = inputField.transform as RectTransform;
                    inputFieldRT.SetParent(inputFieldVariable.transform);
                    inputFieldRT.localScale = Vector3.one;
                    inputFieldRT.anchorMin = new Vector2(0.5f, 0f);
                    inputFieldRT.anchorMax = Vector2.one;
                    inputFieldRT.offsetMin = new Vector2(NodeLayoutLeftPadding, 0f);
                    inputFieldRT.offsetMax = new Vector2(-NodeLayoutRightPadding, 0f);

                    GameObject textObject = new("text");

                    var text = textObject.AddComponent<Text>();
                    text.ApplyRDFont();
                    text.alignment = TextAnchor.LowerLeft;
                    text.color = Color.black;

                    var textRT = textObject.transform as RectTransform;
                    textRT.SetParent(inputFieldRT);
                    textRT.localScale = Vector3.one;
                    textRT.anchorMin = Vector2.zero;
                    textRT.anchorMax = Vector2.one;
                    textRT.offsetMin = new Vector2(InputFieldLeftPadding, 0f);
                    textRT.offsetMax = new Vector2(-InputFieldRightPadding, 0f);

                    field.textComponent = text;
                    variable.inputField = field;
                }

                return inputFieldVariable;
            }
        }

        protected static GameObject BaseVariable
        {
            get
            {
                if (baseVariable == null)
                {
                    baseVariable = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_NodeVariable");
                    baseVariable.SetActive(false);

                    var transform = baseVariable.AddComponent<RectTransform>();
                    transform.anchorMin = Vector2.zero;
                    transform.anchorMax = Vector2.one;
                    transform.offsetMin = transform.offsetMin = Vector2.zero;

                    GameObject name = new("name");
                    Text text = name.AddComponent<Text>();
                    text.ApplyRDFont();
                    text.alignment = TextAnchor.MiddleLeft;

                    text.gameObject.AddComponent<EightSidedOutline>().effectColor = Color.black;

                    baseVariable.AddComponent<TextLayoutElementPropagator>()
                        .Setup(text)
                        .SetHeightPadding(2f);

                    var textTransform = name.transform as RectTransform;
                    textTransform.SetParent(transform);
                    textTransform.anchorMin = Vector2.zero;
                    textTransform.anchorMax = new Vector2(0.5f, 1f);
                    textTransform.offsetMin = new Vector2(NodeLayoutLeftPadding, 0f);
                    textTransform.offsetMax = new Vector2(-NodeLayoutRightPadding, 0f);

                    var variable = baseVariable.AddComponent<NodeVariableDerivative>();
                    variable.rectTransform = transform;
                    variable.text = text;
                }

                return baseVariable;
            }
        }

        private static GameObject baseVariable;
        private static GameObject inputFieldTextPlaceholderVariable;
        private static GameObject inputFieldVariable;
    }
}
