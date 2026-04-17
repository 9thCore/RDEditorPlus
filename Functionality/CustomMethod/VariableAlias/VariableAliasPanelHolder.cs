using RDEditorPlus.Functionality.ArbitraryPanel;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.CustomMethod.VariableAlias
{
    internal class VariableAliasPanelHolder : ArbitraryPanelHolder
    {
        private static VariableAliasPanelHolder instance;
        public static VariableAliasPanelHolder Instance
        {
            get
            {
                if (instance == null || !instance.Valid())
                {
                    instance = new();
                }

                return instance;
            }
        }

        public override void OnHide()
        {

        }

        public override void OnShow()
        {
            scrollbar.value = 1f;
            UpdateUI();
        }

        private void UpdateUI()
        {
            int index = 0;
            var displayAliasData = VariableAliasManager.Instance.GetDisplayAliasData();

            if (aliasDescriptors.Count < displayAliasData.Count)
            {
                for (int i = aliasDescriptors.Count; i < displayAliasData.Count; i++)
                {
                    aliasDescriptors.Add(CreateAliasDescriptor(aliasDescriptors.Count));
                }
            }

            for (int i = displayAliasData.Count; i < aliasDescriptors.Count; i++)
            {
                aliasDescriptors[i].SetActive(false);
            }

            foreach (var data in displayAliasData)
            {
                aliasDescriptors[index].SetActive(true);
                aliasDescriptors[index].SetDownVisibility(true);
                aliasDescriptors[index++].Set(data.Alias, data.Expression);
            }

            if (TryFindLastActiveAliasDescriptor(out var aliasDescriptor))
            {
                aliasDescriptor.SetDownVisibility(false);
            }

            UpdateContentSize();
        }

        private void UpdateContentSize()
        {
            if (!TryFindLastActiveAliasDescriptor(out var aliasDescriptor))
            {
                contentRT.SizeDeltaY(0f);
                creatorAliasDescriptor.MoveTo(AliasStartY + ArbitraryNewAliasOffset + 1.5f);
                return;
            }

            var position = aliasDescriptor.Position;
            creatorAliasDescriptor.MoveTo(position + ArbitraryNewAliasOffset);

            float size = Math.Max(0f, AliasBeginScrollY - position);
            contentRT.SizeDeltaY(size);
        }

        private bool AnyOtherAliasesWithThisName(BaseAliasDescriptor aliasDescriptor)
            => aliasDescriptors.Any(descriptor => descriptor != aliasDescriptor && descriptor.SameAlias(aliasDescriptor));

        private bool TryFindLastActiveAliasDescriptor(out AliasDescriptor aliasDescriptor)
        {
            if (!aliasDescriptors.Any(aliasDescriptor => aliasDescriptor.Active))
            {
                aliasDescriptor = default;
                return false;
            }

            aliasDescriptor = aliasDescriptors.Last(aliasDescriptor => aliasDescriptor.Active);
            return true;
        }

        private void RemoveAlias(int index)
        {
            using (new SaveStateScope())
            {
                VariableAliasManager.Instance.Remove(index);
                UpdateUI();
            }
        }

        private void UpdateAlias(int index, string alias)
        {
            using (new SaveStateScope())
            {
                VariableAliasManager.Instance.SetAlias(index, alias);
                UpdateUI();
            }
        }

        private void UpdateExpression(int index, string expression)
        {
            using (new SaveStateScope())
            {
                VariableAliasManager.Instance.SetExpression(index, expression);
                UpdateUI();
            }
        }

        private void CreateAlias(string alias)
        {
            using (new SaveStateScope())
            {
                VariableAliasManager.Instance.CreateAlias(alias, string.Empty);
                UpdateUI();
            }
        }

        private void Move(int index, int delta)
        {
            int other = index + delta;
            if (other < 0 || other >= aliasDescriptors.Count)
            {
                return;
            }

            using (new SaveStateScope())
            {
                VariableAliasManager.Instance.Swap(index, other);
                UpdateUI();
            }
        }

        private AliasDescriptor CreateAliasDescriptor(int index)
        {
            CreateBaseAliasDescriptorComponents(ref lastAliasPosition, out var holderRT, out var name, out var expression, out var equals);

            CreateButton(holderRT, "delete", AssetUtil.PulseTrashSprite, new Vector2(AliasLeftEdgePadding / 2f, AliasHeight / 2f),
                AliasDeleteIconSize, AliasDeleteAnchor, AliasDeleteIconPadding, DeleteColor, () => RemoveAlias(index), out var delete, out _);
            delete.colors = DeleteColorBlock;

            CreateButton(holderRT, "down", AssetUtil.RowDownArrowSprite,
                    new Vector2(-BaseAliasRightEdgePadding - AliasOrderIconSize / 2f, AliasHeight / 2f), AliasOrderIconSize,
                    AliasOrderAnchor, AliasOrderIconPadding, ReorderColor, () => Move(index, 1), out var down, out _);
            down.colors = ReorderColorBlock;

            Button up = null;
            if (index > 0)
            {

                CreateButton(holderRT, "up", AssetUtil.RowDownArrowSprite,
                    new Vector2(-AliasRightEdgePadding + AliasOrderIconSize / 2f, AliasHeight / 2f), AliasOrderIconSize,
                    AliasOrderAnchor, AliasOrderIconPadding, ReorderColor, () => Move(index, -1), out up, out var image);
                up.colors = ReorderColorBlock;

                image.transform.Rotate(0f, 0f, 180f);
            }

            return new AliasDescriptor(index, name, expression, delete, equals, up, down, this);
        }

        private void CreateButton(RectTransform parent, string name, Sprite sprite, Vector2 position, float size, float anchor,
            float padding, Color color, UnityAction onClick, out Button button, out Image icon)
        {
            GameObject backgroundHolder = new(name);

            var background = backgroundHolder.AddComponent<Image>();
            background.color = color;
            background.type = Image.Type.Tiled;
            background.sprite = AssetUtil.ButtonSprite;

            var backgroundRT = background.rectTransform;
            backgroundRT.SetParent(parent, worldPositionStays: false);
            backgroundRT.anchorMin = backgroundRT.anchorMax = new Vector2(anchor, 0f);
            backgroundRT.sizeDelta = new Vector2(size, size);
            backgroundRT.anchoredPosition = position;

            button = backgroundHolder.AddComponent<Button>();
            button.onClick.AddListener(onClick);

            GameObject iconHolder = new("icon");

            icon = iconHolder.AddComponent<Image>();
            icon.raycastTarget = false;
            icon.color = Color.white;
            icon.sprite = sprite;

            var iconRT = icon.rectTransform;
            iconRT.SetParent(backgroundRT, worldPositionStays: false);
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(padding, padding);
            iconRT.offsetMax = new Vector2(-padding, -padding);
        }

        private void CreateBaseAliasDescriptorComponents(ref float position, out RectTransform holderRT, out InputField name, out InputField expression, out Text equals)
        {
            GameObject holder = new("alias");
            holderRT = holder.AddComponent<RectTransform>();

            holderRT.SetParent(contentRT, worldPositionStays: false);
            holderRT.anchorMin = new Vector2(0f, 1f);
            holderRT.anchorMax = Vector2.one;
            holderRT.offsetMin = new Vector2(0f, position - AliasHeight);
            holderRT.offsetMax = new Vector2(0f, position);

            position -= AliasSeparation + AliasHeight;

            UnityUtil.CreateInputField(holderRT, out name, out var nameRT, TextPadding);
            UnityUtil.CreateInputField(holderRT, out expression, out var expressionRT, TextPadding);

            InspectorUtil.SetupMixedPlaceholder(expression, NullExpressionPlaceholder).color = Color.black.WithAlpha(NullExpressionPlaceholderAlpha);

            name.characterValidation = InputField.CharacterValidation.Alphanumeric;
            name.onValidateInput = OnValidateInput;

            GameObject equalsHolder = new("equals");

            equals = equalsHolder.AddComponent<Text>();
            equals.raycastTarget = false;
            equals.ApplyRDFont();
            equals.color = Color.white;
            equals.text = "=";
            equals.alignment = TextAnchor.MiddleCenter;

            var equalsRT = equalsHolder.transform as RectTransform;
            equalsRT.SetParent(holderRT, worldPositionStays: false);
            equalsRT.anchorMin = new Vector2(AliasNameRealEstate, 0f);
            equalsRT.anchorMax = new Vector2(AliasNameRealEstate, 1f);
            equalsRT.offsetMin = new Vector2(-20f, -20f);
            equalsRT.offsetMax = new Vector2(20f, 20f);

            nameRT.offsetMin = new Vector2(AliasLeftEdgePadding, 0f);
            nameRT.offsetMax = new Vector2(-AliasCenterPadding, 0f);

            expressionRT.offsetMin = new Vector2(AliasCenterPadding, 0f);
            expressionRT.offsetMax = new Vector2(-AliasRightEdgePadding, 0f);

            nameRT.anchorMax = new Vector2(AliasNameRealEstate, 1f);
            expressionRT.anchorMin = new Vector2(AliasNameRealEstate, 0f);
        }

        private VariableAliasPanelHolder()
        {
            title.text = "Variable Alias Setup";
            title.color = Color.yellow;

            UnityUtil.CreateScrollView(rectTransform, out var scrollRect, out contentRT);
            scrollbar = scrollRect.verticalScrollbar;

            GameObject aliasHolder = new("alias");

            var alias = aliasHolder.AddComponent<Text>();
            alias.ApplyRDFont();
            alias.text = "Alias";
            alias.color = Color.white;
            alias.alignment = TextAnchor.UpperCenter;

            var aliasRT = aliasHolder.transform as RectTransform;
            aliasRT.SetParent(contentRT, worldPositionStays: false);
            aliasRT.anchorMin = new Vector2(0f, 1f);
            aliasRT.anchorMax = new Vector2(AliasNameRealEstate, 1f);
            aliasRT.offsetMin = new Vector2(AliasLeftEdgePadding, -AliasDescriptionHeight - 2f);
            aliasRT.offsetMax = new Vector2(-AliasCenterPadding, -2f);

            GameObject expressionHolder = new("expression");

            var expression = expressionHolder.AddComponent<Text>();
            expression.ApplyRDFont();
            expression.text = "Expression";
            expression.color = Color.white;
            expression.alignment = TextAnchor.UpperCenter;

            var expressionRT = expressionHolder.transform as RectTransform;
            expressionRT.SetParent(contentRT, worldPositionStays: false);
            expressionRT.anchorMin = new Vector2(AliasNameRealEstate, 1f);
            expressionRT.anchorMax = Vector2.one;
            expressionRT.offsetMin = new Vector2(AliasCenterPadding, -AliasDescriptionHeight - 2f);
            expressionRT.offsetMax = new Vector2(AliasRightEdgePadding, -2f);

            float position = AliasStartY;
            CreateBaseAliasDescriptorComponents(ref position, out _, out var name, out var creatorExpression, out var equals);
            creatorAliasDescriptor = new CreatorAliasDescriptor(name, creatorExpression, equals, this);
        }

        private float lastAliasPosition = AliasStartY;

        private readonly RectTransform contentRT;
        private readonly Scrollbar scrollbar;
        private readonly CreatorAliasDescriptor creatorAliasDescriptor;
        private readonly List<AliasDescriptor> aliasDescriptors = [];

        private readonly static Vector4 TextPadding = new(2f, 0f, -2f, 0f);
        private readonly static Color DeleteColor = Color.white;
        private readonly static Color ReorderColor = Color.white;
        private readonly static ColorBlock DeleteColorBlock = new()
        {
            normalColor = "C02020FF".HexToColor(),
            highlightedColor = "FF3131FF".HexToColor(),
            pressedColor = "9B1A1AFF".HexToColor(),
            selectedColor = "C04040FF".HexToColor(),
            disabledColor = "A7ABABFF".HexToColor(),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };
        private readonly static ColorBlock ReorderColorBlock = new()
        {
            normalColor = "9B9B9BFF".HexToColor(),
            highlightedColor = "BDBDBDFF".HexToColor(),
            pressedColor = "242424FF".HexToColor(),
            selectedColor = "BDBDBDFF".HexToColor(),
            disabledColor = "C8C8C8FF".HexToColor(),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        private static char OnValidateInput(string _, int index, char value)
            => (index != 0 || DisallowedFirstCharacters.IndexOf(value) == -1) ? value : '\0';

        private const float AliasSeparation = 2f;
        private const float AliasDescriptionHeight = 12f;
        private const float AliasStartY = -AliasDescriptionHeight - AliasSeparation;
        private const float AliasHeight = 11f;
        private const float AliasLeftEdgePadding = 2f + AliasDeleteIconSize + AliasDeleteSeparation;
        private const float AliasRightEdgePadding = BaseAliasRightEdgePadding + AliasOrderIconSize * 2f + AliasOrderSeparation;
        private const float AliasCenterPadding = 4f;
        private const float AliasNameRealEstate = 0.4f;
        private const float AliasBeginScrollY = -130f;
        private const float AliasDeleteIconSize = AliasHeight;
        private const float AliasDeleteIconPadding = 1f;
        private const float AliasDeleteAnchor = 0f;
        private const float AliasDeleteSeparation = 2f;
        private const float AliasOrderIconSize = AliasHeight;
        private const float AliasOrderIconPadding = 3f;
        private const float AliasOrderSeparation = 2f;
        private const float AliasOrderAnchor = 1f;
        private const float BaseAliasRightEdgePadding = 11f;
        private const float ArbitraryNewAliasOffset = -7.5f;

        private const float NullExpressionPlaceholderAlpha = 0.25f;
        private const float AddAliasPlaceholderAlpha = 0.25f;

        private const string DisallowedFirstCharacters = "0123456789";
        private const string NullExpressionPlaceholder = "0";
        private const string AddAliasPlaceholder = "New...";

        private class CreatorAliasDescriptor : BaseAliasDescriptor
        {
            public CreatorAliasDescriptor(InputField name, InputField expression, Text equals, VariableAliasPanelHolder panel)
                : base(name, expression, equals, panel)
            {
                InspectorUtil.SetupMixedPlaceholder(Name, AddAliasPlaceholder).color = Color.black.WithAlpha(AddAliasPlaceholderAlpha);

                SetRightHandSideActive(false);
                Expression.interactable = false;
                Name.onValueChanged.AddListener(OnAliasChanged);

                transform = Name.transform.parent as RectTransform;
            }

            public void MoveTo(float position)
            {
                transform.AnchorPosY(position);
            }

            protected override void OnEndAliasEdit(bool successful)
            {
                SetRightHandSideActive(false);

                if (!successful)
                {
                    Name.SetTextWithoutNotify(string.Empty);
                    return;
                }

                Panel.CreateAlias(Alias);
                Name.SetTextWithoutNotify(string.Empty);
            }

            protected override void OnEndExpressionEdit()
            {
                Plugin.LogError($"Expression of {nameof(CreatorAliasDescriptor)} was edited, but this should not be able to happen!");
                Expression.SetTextWithoutNotify(string.Empty);
            }

            private void OnAliasChanged(string text)
            {
                SetRightHandSideActive(!text.IsNullOrEmpty());
            }

            private void SetRightHandSideActive(bool active)
            {
                Expression.gameObject.SetActive(active);
                Equal.gameObject.SetActive(active);
            }

            private readonly RectTransform transform;
        }

        private class AliasDescriptor(int index, InputField name, InputField expression, Button delete, Text equals,
            Button up, Button down, VariableAliasPanelHolder panel)
            : BaseAliasDescriptor(name, expression, equals, panel)
        {
            public int Index { get; init; } = index;
            public Button Delete { get; init; } = delete;
            public Button Up { get; init; } = up;
            public Button Down { get; init; } = down;

            public void SetDownVisibility(bool visible)
            {
                Down.gameObject.SetActive(visible);
            }

            public override void SetActive(bool active)
            {
                base.SetActive(active);
                Delete.gameObject.SetActive(active);
                Equal.gameObject.SetActive(active);
                Up?.gameObject.SetActive(active);
            }

            public override void Set(string name, string expression)
            {
                base.Set(name, expression);
                lastValidAlias = name;
            }

            protected override void OnEndAliasEdit(bool successful)
            {
                if (!successful)
                {
                    Name.SetTextWithoutNotify(lastValidAlias);
                    Name.textComponent.color = Color.black;
                }
                else
                {
                    lastValidAlias = Alias;
                }

                Panel.UpdateAlias(Index, Alias);
            }

            protected override void OnEndExpressionEdit()
            {
                Panel.UpdateExpression(Index, Value);
            }

            private string lastValidAlias = string.Empty;
        }

        private abstract class BaseAliasDescriptor
        {
            public BaseAliasDescriptor(InputField name, InputField expression, Text equals, VariableAliasPanelHolder panel)
            {
                Name = name;
                Position = (name.transform.parent as RectTransform).offsetMin.y;
                Expression = expression;
                Panel = panel;
                Equal = equals;

                Name.onValueChanged.AddListener(OnAliasEdit);

                Name.onEndEdit.AddListener(OnAliasEndEdit);
                Expression.onEndEdit.AddListener(OnExpressionEndEdit);
            }

            public InputField Name { get; init; }
            public InputField Expression { get; init; }
            public Text Equal { get; init; }
            public float Position { get; init; }

            public bool Active => Name.gameObject.activeSelf;
            public bool Valid => !Name.text.IsNullOrEmpty();
            public string Alias => Name.text;
            public string Value => Expression.text.IsNullOrEmpty() ? NullExpressionPlaceholder : Expression.text;

            public virtual void SetActive(bool active)
            {
                Name.gameObject.SetActive(active);
                Expression.gameObject.SetActive(active);
            }

            public virtual void Set(string name, string expression)
            {
                Name.SetTextWithoutNotify(name);
                Expression.SetTextWithoutNotify(expression);
            }

            public bool SameAlias(BaseAliasDescriptor other) => Alias == other.Alias;

            protected VariableAliasPanelHolder Panel { get; init; }

            protected abstract void OnEndAliasEdit(bool successful);
            protected abstract void OnEndExpressionEdit();

            private void OnAliasEdit(string text)
            {
                if (!VariableAliasManager.Instance.Validate(text, out _))
                {
                    Name.textComponent.color = Color.red;
                    return;
                }

                if (Panel.AnyOtherAliasesWithThisName(this))
                {
                    Name.textComponent.color = Color.Lerp(Color.yellow, Color.black, 0.33f);
                    return;
                }

                Name.textComponent.color = Color.black;
            }

            private void OnAliasEndEdit(string text)
            {
                OnEndAliasEdit(successful: VariableAliasManager.Instance.Validate(text, out _) && !Panel.AnyOtherAliasesWithThisName(this));
            }

            private void OnExpressionEndEdit(string text)
            {
                OnEndExpressionEdit();
            }
        }
    }
}
