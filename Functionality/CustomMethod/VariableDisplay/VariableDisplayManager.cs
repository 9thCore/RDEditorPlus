using RDEditorPlus.Functionality.CustomMethod.VariableAlias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RDEditorPlus.Functionality.CustomMethod.VariableDisplay
{
    public class VariableDisplayManager : MonoBehaviour
    {
        public static VariableDisplayManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject holder = new($"mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(VariableDisplayManager)}");
                    instance = holder.AddComponent<VariableDisplayManager>();

                    if (PluginConfig.CustomMethodsVariableAliasEnabled)
                    {
                        VariableAliasManager.Instance.OnDataRefresh += instance.OnAliasDataRefresh;
                    }
                }

                return instance;
            }
        }

        public void Clear() => expressions.Clear();

        public void AddExpression(string expression)
        {
            expressions.Add(expression);
            UpdateBlankPanel();
        }

        public void RemoveExpression(int index)
        {
            expressions.RemoveAt(index);
            UpdateBlankPanel();
        }

        public IEnumerable<Expression> Expressions => expressions;

        public bool HasExpressions() => expressions.Count > 0;

        public void DecodeModData(Dictionary<string, object> data)
        {
            Clear();

            if (data == null
                || !data.TryGetValue(DisplayData, out var value)
                || value is not List<object> list)
            {
                OnDataRefresh?.Invoke();
                return;
            }

            foreach (var expression in list)
            {
                expressions.Add(expression.ToString());
            }

            UpdateBlankPanel();
            OnAliasDataRefresh();
            OnDataRefresh?.Invoke();
        }

        public bool TryConstructJSONData(out string data)
        {
            if (expressions.Count == 0)
            {
                data = default;
                return false;
            }

            StringBuilder sb = new();

            sb.Append($"\"{DisplayData}\": [");

            bool first = true;
            foreach (var expression in expressions)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;

                sb.Append($"\"{expression.Original}\"");
            }

            sb.Append(']');

            data = sb.ToString();
            return true;
        }

        public Action OnDataRefresh;

        public class Expression(string original)
        {
            public bool InvalidExpression { get; set; } = false;
            public string Original
            {
                get => original;
                set
                {
                    if (value == lastOriginal)
                    {
                        return;
                    }

                    lastOriginal = original;
                    original = value;
                    InvalidExpression = false;
                    Expand();
                }
            }
            public string Expanded { get; private set; } = original;

            public bool TryEvaluate(LevelBase currentLevel, out object result)
            {
                if (InvalidExpression)
                {
                    result = default;
                    return false;
                }

                try
                {
                    result = currentLevel.EvalStringWithVariables(Expanded);
                    return true;
                }
                catch (Exception)
                {
                    result = default;
                    InvalidExpression = true;
                    return false;
                }
            }

            public void Expand()
            {
                Expanded = VariableAliasManager.Instance.ExpandAliases(Original, onlyInBraces: false);
            }

            private string original = original;
            private string lastOriginal = original;

            public static implicit operator Expression(string original) => new(original);
        }

        private void UpdateBlankPanel()
        {
            Plugin.LogWarn("TODO update blank panel");
        }

        private void OnAliasDataRefresh()
        {
            if (!PluginConfig.CustomMethodsVariableAliasEnabled)
            {
                return;
            }

            foreach (var expression in expressions)
            {
                expression.Expand();
            }
        }

        private readonly List<Expression> expressions = [];

        private static VariableDisplayManager instance;

        private const string DisplayData = "variableDisplay";
    }
}
