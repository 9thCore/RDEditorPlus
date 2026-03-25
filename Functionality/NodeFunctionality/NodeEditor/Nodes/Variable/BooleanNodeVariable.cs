using RDEditorPlus.Util;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable
{
    public class BooleanNodeVariable : NodeVariable<BooleanNodeVariable, bool>
    {
        public override bool CanSave() => CurrentValue != initialValue;

        protected override void OnVariableChange(string text)
        {
            if (bool.TryParse(text, out var result))
            {
                CurrentValue = result;
            }
            else
            {
                CurrentValue = initialValue;
            }

            base.OnVariableChange(text);
        }

        protected override void SetRepresentation(string value)
        {
            if (bool.TryParse(value, out var result))
            {
                toggle.SetIsOnWithoutNotify(result);
            }
            else
            {
                toggle.SetIsOnWithoutNotify(false);
            }
        }

        private void Awake()
        {
            toggle.onValueChanged.AddListener(OnToggleChange);
        }

        private void OnToggleChange(bool value) => OnVariableChange(value.ToString());

        [SerializeField]
        private Toggle toggle;

        public static GameObject VariablePrefab
        {
            get
            {
                if (variablePrefab == null)
                {
                    variablePrefab = Instantiate(BaseVariable);
                    variablePrefab.name += "Boolean";

                    var variable = variablePrefab.GetComponent<BooleanNodeVariable>();
                    variable.type = Node.Type.Boolean;

                    GameObject toggleHolder = new("toggle");

                    var toggleRT = toggleHolder.AddComponent<RectTransform>();
                    toggleRT.SetParent(variablePrefab.transform);
                    toggleRT.pivot = new Vector2(1f, 0.5f);
                    toggleRT.localPosition = Vector2.zero;
                    toggleRT.localScale = Vector3.one;
                    toggleRT.anchorMin = new Vector2(1f, 0f);
                    toggleRT.anchorMax = Vector2.one;
                    toggleRT.sizeDelta = new Vector2(9f, 0f);
                    toggleRT.anchoredPosition += Vector2.left * 8f;

                    GameObject background = new("background");

                    var backgroundImage = background.AddComponent<Image>();
                    backgroundImage.type = Image.Type.Tiled;
                    backgroundImage.sprite = AssetUtil.ButtonSprite;

                    var backgroundRT = background.transform as RectTransform;
                    backgroundRT.SetParent(toggleRT);
                    backgroundRT.localScale = Vector3.one;
                    backgroundRT.anchorMin = Vector2.zero;
                    backgroundRT.anchorMax = Vector2.one;
                    backgroundRT.offsetMin = backgroundRT.offsetMax = Vector2.zero;

                    GameObject graphic = new("graphic");

                    var graphicImage = graphic.AddComponent<Image>();
                    graphicImage.sprite = AssetUtil.CheckmarkSprite;
                    graphicImage.color = Color.Lerp(Color.black, Color.white, 0.25f);

                    var graphicRT = graphic.transform as RectTransform;
                    graphicRT.SetParent(backgroundRT);
                    graphicRT.localScale = Vector3.one;
                    graphicRT.anchorMin = graphicRT.anchorMax = new Vector2(0.5f, 0.5f);
                    graphicRT.offsetMin = graphicRT.offsetMax = Vector2.zero;
                    graphicRT.sizeDelta = new Vector2(9f, 9f);

                    var toggle = toggleHolder.AddComponent<Toggle>();
                    toggle.toggleTransition = Toggle.ToggleTransition.Fade;
                    toggle.graphic = graphicImage;
                    toggle.targetGraphic = backgroundImage;

                    variable.toggle = toggle;
                }

                return variablePrefab;
            }
        }

        private static GameObject variablePrefab;
    }
}
