using RDEditorPlus.Functionality.ArbitraryPanel;
using RDEditorPlus.Functionality.Mixins;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.CustomMethod.VariableDisplay
{
    public class VariableDisplayPanelHolder : ArbitraryPanelHolder, IExtensibleUIList<VariableDisplayPanelHolder.ExpressionDescriptor>
    {
        private static VariableDisplayPanelHolder instance;
        public static VariableDisplayPanelHolder Instance
        {
            get
            {
                if (instance == null || !instance.Valid())
                {
                    instance = new();
                    VariableDisplayManager.Instance.OnDataRefresh += instance.UpdateUI;
                }

                return instance;
            }
        }

        public override void OnHide()
        {

        }

        public override void OnShow()
        {
            UpdateUI();
        }

        IList<ExpressionDescriptor> IExtensibleUIList<ExpressionDescriptor>.VariableList => expressions;
        ExpressionDescriptor IExtensibleUIList<ExpressionDescriptor>.CreateElement(int index) => CreateExpression(index);
        RectTransform IExtensibleUIList<ExpressionDescriptor>.ContentRectTransform => contentRT;
        IExtensibleUIList<ExpressionDescriptor>.IListCreatorElement IExtensibleUIList<ExpressionDescriptor>.CreatorElement => creatorExpression;

        private void Add(string expression)
        {
            using (new SaveStateScope())
            {
                VariableDisplayManager.Instance.AddExpression(expression);
                UpdateUI();
            }
        }

        private void Remove(int index)
        {
            using (new SaveStateScope())
            {
                VariableDisplayManager.Instance.RemoveExpression(index);
                UpdateUI();
            }
        }

        private void Update(int index, string expression)
        {
            using (new SaveStateScope())
            {
                VariableDisplayManager.Instance.UpdateExpression(index, expression);
                UpdateUI();
            }
        }

        private void Move(int index, int delta)
        {
            using (new SaveStateScope())
            {
                VariableDisplayManager.Instance.Swap(index, index + delta);
                UpdateUI();
            }
        }

        private void MoveUp(int index) => Move(index, -1);
        private void MoveDown(int index) => Move(index, 1);

        private ExpressionDescriptor CreateExpression(int index)
        {
            CreateBaseExpressionDescriptorComponents(ref lastPosition, out var holderRT, out var expression);

            this.CreateButtons(holderRT, index, LeftEdgePadding, RightEdgePadding, ExpressionHeight, ButtonSpacing, DeleteIconPadding,
                OrderIconPadding, DeleteColorBlock, ReorderColorBlock, () => Remove(index), () => MoveDown(index), () => MoveUp(index),
                out var delete, out var up, out var down);

            return new ExpressionDescriptor(index, expression, delete, up, down, this);
        }

        private void CreateBaseExpressionDescriptorComponents(ref float position, out RectTransform holderRT, out InputField expression)
        {
            GameObject holder = new("expression");
            holderRT = holder.AddComponent<RectTransform>();

            holderRT.SetParent(contentRT, worldPositionStays: false);
            holderRT.anchorMin = new Vector2(0f, 1f);
            holderRT.anchorMax = Vector2.one;
            holderRT.offsetMin = new Vector2(0f, position - ExpressionHeight);
            holderRT.offsetMax = new Vector2(0f, position);

            position -= ExpressionSpacing + ExpressionHeight;

            UnityUtil.CreateInputField(holderRT, out expression, out var expressionRT, TextPadding);

            expressionRT.offsetMin = new Vector2(LeftEdgePadding, 0f);
            expressionRT.offsetMax = new Vector2(-RightEdgePadding, 0f);
        }

        private void UpdateContentSize() => this.UpdateContentSize(CreatorStartY, ArbitraryCreatorOffset, BeginScrollY);

        private void UpdateUI()
        {
            var list = VariableDisplayManager.Instance.Expressions;

            this.EnsureListElements(list.Count);

            int index = 0;
            foreach (var expression in list)
            {
                expressions[index++].Set(expression.Original);
            }

            UpdateContentSize();
        }

        private VariableDisplayPanelHolder()
        {
            title.text = "Variable Display Setup";
            title.color = Color.yellow;

            UnityUtil.CreateScrollView(rectTransform, out _, out contentRT);

            GameObject expressionHolder = new("expression");

            var expression = expressionHolder.AddComponent<Text>();
            expression.ApplyRDFont();
            expression.text = "Expression";
            expression.color = Color.white;
            expression.alignment = TextAnchor.UpperCenter;

            var expressionRT = expressionHolder.transform as RectTransform;
            expressionRT.SetParent(contentRT, worldPositionStays: false);
            expressionRT.anchorMin = new Vector2(0f, 1f);
            expressionRT.anchorMax = Vector2.one;
            expressionRT.offsetMin = new Vector2(LeftEdgePadding, -DescriptionHeight - 2f);
            expressionRT.offsetMax = new Vector2(-RightEdgePadding, -2f);

            float position = StartY;
            CreateBaseExpressionDescriptorComponents(ref position, out _, out var creatorExpression);
            this.creatorExpression = new ExpressionCreatorDescriptor(creatorExpression, this);
        }

        private float lastPosition = 0f;

        private readonly List<ExpressionDescriptor> expressions = [];
        private readonly ExpressionCreatorDescriptor creatorExpression;
        private readonly RectTransform contentRT;

        private readonly static Vector4 TextPadding = new(2f, 0f, -2f, 0f);
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

        private const float Separation = 2f;
        private const float DescriptionHeight = 12f;

        private const float StartY = -DescriptionHeight - Separation;
        private const float CreatorStartY = StartY + 1.5f;
        private const float ArbitraryCreatorOffset = -7.5f;
        private const float BeginScrollY = -130f;

        private const float ExpressionHeight = 11f;
        private const float ExpressionSpacing = 2f;

        private const float ButtonSpacing = 2f;
        private const float DeleteIconPadding = 1f;
        private const float OrderIconPadding = 3f;

        private const float LeftEdgePadding = 2f + ExpressionHeight + ButtonSpacing;
        private const float RightEdgePadding = 11f + ExpressionHeight * 2f + ButtonSpacing * 2f;

        private const float CreatorPlaceholderAlpha = 0.25f;
        private const string CreatorPlaceholderText = "New...";

        private class ExpressionDescriptor(int index, InputField expression, Button delete, Button up, Button down, VariableDisplayPanelHolder panel)
            : BaseExpressionDescriptor(expression, panel), IExtensibleUIList<ExpressionDescriptor>.IListElement
        {
            public int Index { get; init; } = index;
            public Button Delete { get; init; } = delete;
            public Button Up { get; init; } = up;
            public Button Down { get; init; } = down;

            public void SetDownArrowVisibility(bool visible)
            {
                Down.gameObject.SetActive(visible);
            }

            public override void SetActive(bool active)
            {
                base.SetActive(active);
                Delete.gameObject.SetActive(active);
                Up?.gameObject.SetActive(active);
            }

            protected override void OnEndExpressionEdit()
            {
                Panel.Update(Index, Value);
            }
        }

        private class ExpressionCreatorDescriptor : BaseExpressionDescriptor, IExtensibleUIList<ExpressionDescriptor>.IListCreatorElement
        {
            public ExpressionCreatorDescriptor(InputField expression, VariableDisplayPanelHolder panel) : base(expression, panel)
            {
                InspectorUtil.SetupMixedPlaceholder(Expression, CreatorPlaceholderText).color = Color.black.WithAlpha(CreatorPlaceholderAlpha);

                transform = expression.transform.parent as RectTransform;
            }

            public void MoveTo(float position)
            {
                transform.AnchorPosY(position);
            }

            protected override void OnEndExpressionEdit()
            {
                Panel.Add(Value);
                Expression.SetTextWithoutNotify(string.Empty);
            }

            private readonly RectTransform transform;
        }

        private abstract class BaseExpressionDescriptor
        {
            public BaseExpressionDescriptor(InputField expression, VariableDisplayPanelHolder panel)
            {
                Expression = expression;
                Position = (expression.transform.parent as RectTransform).offsetMin.y;
                Panel = panel;

                Expression.onEndEdit.AddListener(OnExpressionEndEdit);
            }

            public virtual void SetActive(bool active)
            {
                Expression.gameObject.SetActive(active);
            }

            public InputField Expression { get; init; }
            public float Position { get; init; }

            public bool Active => Expression.gameObject.activeSelf;
            public string Value => Expression.text;

            public void Set(string text)
            {
                Expression.text = text;
            }

            protected VariableDisplayPanelHolder Panel { get; init; }
            protected abstract void OnEndExpressionEdit();

            private void OnExpressionEndEdit(string text)
            {
                OnEndExpressionEdit();
            }
        }
    }
}
