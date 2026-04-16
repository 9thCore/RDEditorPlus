using RDEditorPlus.Functionality.ArbitraryPanel;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

            UpdateContentSize();

            foreach (var data in displayAliasData)
            {
                aliasDescriptors[index].SetActive(true);
                aliasDescriptors[index++].Set(data.Alias, data.Expression);
            }
        }

        private void UpdateContentSize()
        {
            if (!aliasDescriptors.Any(descriptor => descriptor.Active))
            {
                contentRT.SizeDeltaY(0f);
                creatorAliasDescriptor.MoveTo(AliasStartY);
                return;
            }

            var position = aliasDescriptors.Last(descriptor => descriptor.Active).Position;
            creatorAliasDescriptor.MoveTo(position - 7.5f);

            float size = Math.Max(0f, AliasBeginScrollY - position);
            contentRT.SizeDeltaY(size);
        }

        private bool AnyOtherAliasesWithThisName(BaseAliasDescriptor aliasDescriptor)
            => aliasDescriptors.Any(descriptor => descriptor != aliasDescriptor && descriptor.SameAlias(aliasDescriptor));

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

        private AliasDescriptor CreateAliasDescriptor(int index)
        {
            CreateBaseAliasDescriptorComponents(ref lastAliasPosition, out var holderRT, out var name, out var expression, out var equals);

            GameObject deleteBackgroundHolder = new("delete");

            var deleteBackground = deleteBackgroundHolder.AddComponent<Image>();
            deleteBackground.color = Color.red;
            deleteBackground.type = Image.Type.Tiled;
            deleteBackground.sprite = AssetUtil.ButtonSprite;

            var deleteBackgroundRT = deleteBackground.rectTransform;
            deleteBackgroundRT.SetParent(holderRT, worldPositionStays: false);
            deleteBackgroundRT.anchorMin = deleteBackgroundRT.anchorMax = Vector2.zero;
            deleteBackgroundRT.sizeDelta = new Vector2(AliasDeleteIconSize, AliasDeleteIconSize);
            deleteBackgroundRT.anchoredPosition = new Vector2(AliasLeftEdgePadding / 2f, AliasHeight / 2f);

            var delete = deleteBackgroundHolder.AddComponent<Button>();
            delete.onClick.AddListener(() => RemoveAlias(index));

            GameObject deleteIconHolder = new("icon");

            var deleteIcon = deleteIconHolder.AddComponent<Image>();
            deleteIcon.raycastTarget = false;
            deleteIcon.color = Color.white;
            deleteIcon.sprite = AssetUtil.PulseTrashSprite;

            var deleteIconRT = deleteIcon.rectTransform;
            deleteIconRT.SetParent(deleteBackgroundRT, worldPositionStays: false);
            deleteIconRT.anchorMin = Vector2.zero;
            deleteIconRT.anchorMax = Vector2.one;
            deleteIconRT.offsetMin = new Vector2(AliasDeleteIconPadding, AliasDeleteIconPadding);
            deleteIconRT.offsetMax = new Vector2(-AliasDeleteIconPadding, -AliasDeleteIconPadding);

            return new AliasDescriptor(index, name, expression, delete, equals, this);
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

        private static char OnValidateInput(string _, int index, char value)
            => (index != 0 || DisallowedFirstCharacters.IndexOf(value) == -1) ? value : '\0';

        private const float AliasSeparation = 2f;
        private const float AliasDescriptionHeight = 12f;
        private const float AliasStartY = -AliasDescriptionHeight - AliasSeparation;
        private const float AliasHeight = 11f;
        private const float AliasLeftEdgePadding = 2f + AliasDeleteIconSize + AliasDeleteSeparation;
        private const float AliasRightEdgePadding = 11f;
        private const float AliasCenterPadding = 4f;
        private const float AliasNameRealEstate = 0.4f;
        private const float AliasBeginScrollY = -130f;
        private const float AliasDeleteIconSize = 11f;
        private const float AliasDeleteIconPadding = 1f;
        private const float AliasDeleteSeparation = 2f;

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

        private class AliasDescriptor(int index, InputField name, InputField expression, Button delete, Text equals, VariableAliasPanelHolder panel)
            : BaseAliasDescriptor(name, expression, equals, panel)
        {
            public int Index { get; init; } = index;
            public Button Delete { get; init; } = delete;

            public override void SetActive(bool active)
            {
                base.SetActive(active);
                Delete.gameObject.SetActive(active);
                Equal.gameObject.SetActive(active);
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
