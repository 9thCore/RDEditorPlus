using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable.Enums
{
    public abstract class EnumNodeVariable<ScriptType, TEnum> : NodeVariable<ScriptType, TEnum>
        where ScriptType : EnumNodeVariable<ScriptType, TEnum>
        where TEnum : struct, Enum
    {
        protected override void OnVariableChange(string text)
        {
            if (Enum.TryParse(text, out TEnum value))
            {
                currentValue = value;
            }
            else
            {
                currentValue = initialValue;
            }

            base.OnVariableChange(text);
        }

        protected override void SetInitialValue(object initialValue)
        {
            dropdown.value = Convert.ToInt32(initialValue);
        }

        protected override void SetRepresentation(string value)
        {
            dropdown.value = Convert.ToInt32(Enum.Parse(typeof(TEnum), value));
        }

        private void OnVariableChange(int index)
        {
            currentValue = Values[index];
            SetRepresentation(currentValue.ToString());
        }

        private void Awake()
        {
            dropdown.onValueChanged.AddListener(OnVariableChange);
        }

        [SerializeField]
        protected Dropdown dropdown;

        protected static GameObject GetCachedPrefab(Node.Type type)
        {
            if (variablePrefab == null)
            {
                variablePrefab = Instantiate(EnumNodeVariablePrefabHolder.GetCachedPrefab(BaseVariable));

                var variable = variablePrefab.GetComponent<ScriptType>();
                variable.type = type;

                variable.dropdown = variablePrefab.GetComponentInChildren<Dropdown>();
                variable.dropdown.AddOptionsWithEnumValues(typeof(TEnum), localize: false);
            }

            return variablePrefab;
        }

        private static GameObject variablePrefab;

        private static readonly TEnum[] Values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
    }

    // ugly hack lol
    internal static class EnumNodeVariablePrefabHolder
    {
        public static GameObject GetCachedPrefab(GameObject baseVariable)
        {
            if (cachedPrefab == null)
            {
                cachedPrefab = GameObject.Instantiate(GetPrefab(baseVariable));
            }

            return cachedPrefab;
        }

        private static GameObject GetPrefab(GameObject baseVariable)
        {
            GameObject variablePrefab = GameObject.Instantiate(baseVariable);
            variablePrefab.name += "Enum";

            GameObject dropdownHolder = new("dropdown");

            var dropdownImage = dropdownHolder.AddComponent<Image>();
            dropdownImage.type = Image.Type.Tiled;
            dropdownImage.sprite = AssetUtil.ButtonSprite;

            var dropdownRT = dropdownHolder.transform as RectTransform;
            dropdownRT.SetParent(variablePrefab.transform);
            dropdownRT.pivot = new Vector2(1f, 0.5f);
            dropdownRT.localPosition = Vector2.zero;
            dropdownRT.localScale = Vector3.one;
            dropdownRT.anchorMin = new Vector2(0.5f, 0f);
            dropdownRT.anchorMax = Vector2.one;
            dropdownRT.offsetMin = new Vector2(2f, 0f);
            dropdownRT.offsetMax = new Vector2(-2f, 0f);

            #region label
            GameObject caption = new("caption");

            var captionText = caption.AddComponent<Text>();
            captionText.ApplyRDFont();
            captionText.alignment = TextAnchor.MiddleLeft;
            captionText.text = "<unset>";
            captionText.color = Color.black;

            var captionRT = caption.transform as RectTransform;
            captionRT.SetParent(dropdownRT.transform);
            captionRT.localPosition = Vector2.zero;
            captionRT.localScale = Vector3.one;
            captionRT.anchorMin = Vector2.zero;
            captionRT.anchorMax = Vector2.one;
            captionRT.offsetMin = new Vector2(2f, 0f);
            captionRT.offsetMax = new Vector2(-2f, 0f);
            #endregion

            #region arrow
            GameObject arrowHolder = new("arrow");

            var arrowImage = arrowHolder.AddComponent<Image>();
            arrowImage.type = Image.Type.Tiled;
            arrowImage.sprite = AssetUtil.DownArrowSprite;

            var arrowRT = arrowHolder.transform as RectTransform;
            arrowRT.SetParent(dropdownRT);
            arrowRT.pivot = new Vector2(1f, 0.5f);
            arrowRT.localPosition = Vector2.zero;
            arrowRT.localScale = Vector3.one;
            arrowRT.anchorMin = arrowRT.anchorMax = new Vector2(1f, 0.5f);
            arrowRT.sizeDelta = new Vector2(7f, 5f);
            arrowRT.anchoredPosition += new Vector2(-3.5f, -1f);
            #endregion

            #region template
            GameObject templateHolder = new("template");
            templateHolder.SetActive(false);

            var templateImage = templateHolder.AddComponent<Image>();
            templateImage.type = Image.Type.Tiled;
            templateImage.sprite = AssetUtil.ButtonSprite;

            var templateRT = templateHolder.transform as RectTransform;
            templateRT.SetParent(dropdownRT);
            templateRT.pivot = new Vector2(0.5f, 1f);
            templateRT.localPosition = Vector2.zero;
            templateRT.localScale = Vector3.one;
            templateRT.anchorMin = Vector2.zero;
            templateRT.anchorMax = new Vector2(1f, 0f);
            templateRT.offsetMin = new Vector2(0f, -76f);
            templateRT.offsetMax = new Vector2(35f, -1f);
            #endregion

            #region template.viewport
            GameObject viewportHolder = new("viewport");

            var viewportImage = viewportHolder.AddComponent<Image>();
            viewportImage.type = Image.Type.Tiled;
            viewportImage.sprite = AssetUtil.UIMaskSprite;

            viewportHolder.AddComponent<Mask>().showMaskGraphic = false;

            var viewportRT = viewportHolder.transform as RectTransform;
            viewportRT.SetParent(templateRT);
            viewportRT.localPosition = Vector2.zero;
            viewportRT.localScale = Vector3.one;
            viewportRT.pivot = new Vector2(0f, 1f);
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = viewportRT.offsetMax = new Vector2(-2f, 0f);
            #endregion

            #region template.viewport.content
            GameObject contentHolder = new("content");

            var contentRT = contentHolder.AddComponent<RectTransform>();
            contentRT.SetParent(viewportRT);
            contentRT.localPosition = Vector2.zero;
            contentRT.localScale = Vector3.one;
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = new Vector2(0f, -17f);
            contentRT.offsetMax = Vector2.zero;
            #endregion

            #region template.viewport.content.item
            GameObject itemHolder = new("item");

            var itemToggle = itemHolder.AddComponent<Toggle>();

            var itemRT = itemHolder.transform as RectTransform;
            itemRT.SetParent(contentRT);
            itemRT.localPosition = Vector2.zero;
            itemRT.localScale = Vector3.one;
            itemRT.anchorMin = new Vector2(0f, 0.5f);
            itemRT.anchorMax = new Vector2(1f, 0.5f);
            itemRT.offsetMin = itemRT.offsetMax = Vector2.zero;
            itemRT.sizeDelta = new Vector2(0f, 15f);

            GameObject itemBackgroundHolder = new("Item Background");

            var itemBackgroundImage = itemBackgroundHolder.AddComponent<Image>();
            itemBackgroundImage.sprite = AssetUtil.Button1PxSprite;
            itemBackgroundImage.type = Image.Type.Tiled;

            var itemBackgroundRT = itemBackgroundHolder.transform as RectTransform;
            itemBackgroundRT.SetParent(itemRT);
            itemBackgroundRT.localPosition = Vector2.zero;
            itemBackgroundRT.localScale = Vector3.one;
            itemBackgroundRT.anchorMin = Vector2.zero;
            itemBackgroundRT.anchorMax = Vector2.one;
            itemBackgroundRT.offsetMin = new Vector2(2f, 1f);
            itemBackgroundRT.offsetMax = new Vector2(-4f, -1f);

            GameObject itemCheckmarkHolder = new("Item Checkmark");

            var itemCheckmarkImage = itemCheckmarkHolder.AddComponent<Image>();
            itemCheckmarkImage.sprite = AssetUtil.CLSSignsArrowSprite;
            itemCheckmarkImage.color = Color.black;

            var itemCheckmarkRT = itemCheckmarkHolder.transform as RectTransform;
            itemCheckmarkRT.SetParent(itemRT);
            itemCheckmarkRT.localPosition = Vector2.zero;
            itemCheckmarkRT.localScale = Vector3.one;
            itemCheckmarkRT.anchorMin = itemCheckmarkRT.anchorMax = new Vector2(1f, 0.5f);
            itemCheckmarkRT.sizeDelta = new Vector2(7f, 7f);
            itemCheckmarkRT.anchoredPosition += Vector2.left * 12f;

            GameObject itemLabelHolder = new("Item Label");

            var itemLabelText = itemLabelHolder.AddComponent<Text>();
            itemLabelText.ApplyRDFont();
            itemLabelText.text = "Option A";
            itemLabelText.color = Color.black;
            itemLabelText.alignment = TextAnchor.MiddleLeft;

            var itemLabelRT = itemLabelHolder.transform as RectTransform;
            itemLabelRT.SetParent(itemRT);
            itemLabelRT.localPosition = Vector2.zero;
            itemLabelRT.localScale = Vector3.one;
            itemLabelRT.pivot = new Vector2(0f, 1f);
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(13f, 0f);
            itemLabelRT.offsetMax = new Vector2(-5f, 0f);

            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = itemCheckmarkImage;
            itemToggle.colors = new()
            {
                normalColor = "FFFFFF00".HexToColor(),
                highlightedColor = "CBEEEEFF".HexToColor(),
                pressedColor = "9CEDF5FF".HexToColor(),
                selectedColor = "82EAF3FF".HexToColor(),
                disabledColor = "C8C8C880".HexToColor()
            };
            #endregion

            #region template.scrollbar
            GameObject scrollbarHolder = new("scrollbar");

            var scrollbarImage = scrollbarHolder.AddComponent<Image>();
            scrollbarImage.type = Image.Type.Tiled;
            scrollbarImage.sprite = AssetUtil.EditorRoundedRectangleSprite;
            scrollbarImage.enabled = false;

            var scrollbarRT = scrollbarHolder.transform as RectTransform;
            scrollbarRT.SetParent(templateRT);
            scrollbarRT.localPosition = Vector2.zero;
            scrollbarRT.localScale = Vector3.one;
            scrollbarRT.pivot = Vector2.one;
            scrollbarRT.anchorMin = new Vector2(1f, 0f);
            scrollbarRT.anchorMax = Vector2.one;
            scrollbarRT.offsetMin = new Vector2(-5f, 2f);
            scrollbarRT.offsetMax = new Vector2(-1f, -2f);

            GameObject slidingAreaHolder = new("slidingArea");

            var slidingAreaRT = slidingAreaHolder.AddComponent<RectTransform>();
            slidingAreaRT.SetParent(scrollbarRT);
            slidingAreaRT.localPosition = Vector2.zero;
            slidingAreaRT.localScale = Vector2.one;
            slidingAreaRT.anchorMin = Vector2.zero;
            slidingAreaRT.anchorMax = Vector2.one;
            slidingAreaRT.offsetMin = new Vector2(5f, 5f);
            slidingAreaRT.offsetMax = new Vector2(-5f, -5f);

            GameObject handleHolder = new("handle");

            var handleImage = handleHolder.AddComponent<Image>();
            handleImage.sprite = AssetUtil.Button1PxSprite;
            handleImage.type = Image.Type.Tiled;
            handleImage.color = "9C9C9CFF".HexToColor();

            var handleRT = handleHolder.transform as RectTransform;
            handleRT.SetParent(slidingAreaRT);
            handleRT.localPosition = Vector2.zero;
            handleRT.localScale = new Vector2(1f, 0.7733f);
            handleRT.anchorMin = Vector2.zero;
            handleRT.anchorMax = Vector2.one;
            handleRT.offsetMin = new Vector2(-5f, -5f);
            handleRT.offsetMax = new Vector2(5f, 5f);

            var scrollbar = scrollbarHolder.AddComponent<Scrollbar>();
            scrollbar.size = 0.7733f;
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRT;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            var scrollRect = templateHolder.AddComponent<ScrollRect>();
            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarSpacing = -2f;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.scrollSensitivity = 10f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.horizontal = false;

            templateHolder.AddComponent<DropdownAutoScroll>();
            #endregion

            var dropdown = dropdownHolder.AddComponent<Dropdown>();
            dropdown.template = templateRT;
            dropdown.captionText = captionText;
            dropdown.itemText = itemLabelText;
            dropdown.targetGraphic = dropdownImage;

            return variablePrefab;
        }

        private static GameObject cachedPrefab;
    }
}
