using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RDEditorPlus.Functionality.CustomMethod.VariableAlias
{
    public class VariableAliasManager : MonoBehaviour
    {
        public static VariableAliasManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject holder = new($"mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(VariableAliasManager)}");
                    instance = holder.AddComponent<VariableAliasManager>();
                }

                return instance;
            }
        }

        public string ExpandAliases(string expression, bool onlyInBraces)
        {
            if (!onlyInBraces)
            {
                return ExpandAliases(expression);
            }

            StringBuilder reconstructor = null;

            var matches = CurlyBraceFormat.Matches(expression);

            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];

                if (TryExpandAliases(match.Value, out string result))
                {
                    (reconstructor ??= new(expression))
                        .Remove(match.Index, match.Length)
                        .Insert(match.Index, result);
                }
            }

            return reconstructor == null ? expression : reconstructor.ToString();
        }

        public List<DisplayAliasData> GetDisplayAliasData()
        {
            List<DisplayAliasData> result = [];

            foreach (var alias in aliasData)
            {
                result.Add(new DisplayAliasData(alias.Alias, alias.Expression));
            }

            return result;
        }

        public void Clear() => aliasData.Clear();
        public void ClearOriginalValues()
        {
            expandedEventsAliasData.Clear();
            expandedCustomConditionalAliasData.Clear();
        }
        public void Remove(int index)
        {
            aliasData.RemoveAt(index);
            UpdateAliasExpansionsFrom(index + 1);
        }

        public void Swap(int index1, int index2)
        {
            aliasData[index1].SwapWith(aliasData[index2]);

            int min = Math.Min(index1, index2);
            int max = Math.Max(index1, index2);

            for (int i = min; i <= max; i++)
            {
                aliasData[i].UpdateExpandedExpression();
            }
        }

        public void SetAlias(int index, string alias)
        {
            aliasData[index].Alias = alias;
            UpdateAliasExpansionsFrom(index + 1);
        }

        public void SetExpression(int index, string expression)
        {
            aliasData[index].Expression = expression;
            UpdateAliasExpansionsFrom(index + 1);
        }

        public bool AliasExists(string alias) => aliasData.Any(data => data.Alias == alias);

        public void CreateAlias(string alias, string expression)
        {
            if (AliasExists(alias))
            {
                return;
            }

            var data = new AliasData(this, alias, expression, aliasData.Count);
            aliasData.Add(data);
        }

        public bool Validate(string alias, out FailureReason reason)
        {
            if (!AliasNameFormat.IsMatch(alias))
            {
                reason = FailureReason.AliasNameIsWrong;
                return false;
            }

            if (bool.TryParse(alias, out _))
            {
                reason = FailureReason.AliasIsBoolean;
                return false;
            }

            if (alias == "Rand")
            {
                reason = FailureReason.AliasIsFieldOrMethod;
                return false;
            }

            var type = typeof(LevelBase);
            if (type.GetField(alias) != null || type.GetProperty(alias) != null || type.GetMethod(alias) != null)
            {
                reason = FailureReason.AliasIsFieldOrMethod;
                return false;
            }

            reason = default;
            return true;
        }

        public void DecodeModData(Dictionary<string, object> data)
        {
            Clear();

            if (data == null)
            {
                OnDataRefresh?.Invoke();
                return;
            }

            if (data.TryGetValue(AliasKey, out var value) && value is List<object> aliasDefinitions)
            {
                foreach (var definition in aliasDefinitions)
                {
                    if (definition is List<object> aliasDefinition && aliasDefinition.Count >= 2)
                    {
                        var alias = aliasDefinition[0].ToString();
                        
                        if (AliasExists(alias))
                        {
                            Plugin.LogInfo($"Could not register alias {alias} because: it already exists");
                        }
                        if (!Validate(alias, out var reason))
                        {
                            Plugin.LogInfo($"Could not register alias {alias} because: {reason}");
                        }
                        else
                        {
                            var expression = aliasDefinition[1].ToString();
                            CreateAlias(alias, expression);
                        }
                    }
                }
            }

            OnDataRefresh?.Invoke();
        }

        public bool TryConstructJSONData(out string data)
        {
            if (aliasData.Count == 0)
            {
                data = default;
                return false;
            }

            StringBuilder sb = new();

            sb.Append($"\"{AliasKey}\": [");

            bool first = true;
            foreach (var alias in aliasData)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;

                sb.Append($"[\"{alias.Alias}\", \"{alias.Expression}\"]");
            }

            sb.Append(']');

            data = sb.ToString();
            return true;
        }

        public void BeforeEventDeserialisation(Dictionary<string, object> dict)
        {
            List<ReplacementValues> storage = [];

            foreach (var kvp in dict)
            {
                if (kvp.Key.StartsWith(AliasDynamicKeyPrefix))
                {
                    var originalKeyPart = kvp.Key.Substring(AliasDynamicKeyPrefix.Length);

                    if (dict.ContainsKey(originalKeyPart))
                    {
                        storage.Add(new(originalKeyPart, kvp.Value));
                    }
                }
            }

            foreach (var replacementValue in storage)
            {
                dict[replacementValue.KeyPart] = replacementValue.Value;
            }
        }

        public void BeforeConditionalDeserialisation(Dictionary<string, object> dict)
        {
            if (!dict.TryGetValue(AliasCustomConditionalExpression, out object value)
                || value is not string text)
            {
                return;
            }

            dict["expression"] = text;
        }

        public void BeforeEventJSONConstruct(LevelEvent_Base levelEvent)
        {
            if (!ExpressionCapableTypes.TryGetValue(levelEvent.GetType(), out var list))
            {
                return;
            }

            foreach (var expressionCapable in list)
            {
                expressionCapable.ExpandAliases(this, levelEvent);
            }
        }

        public void BeforeConditionalJSONConstruct(Conditional conditional)
        {
            if (conditional is not Conditional_Custom customConditional)
            {
                return;
            }

            expandedCustomConditionalAliasData[customConditional.id] = customConditional.customExpression;
            customConditional.customExpression = ExpandAliases(customConditional.customExpression, onlyInBraces: false);
        }

        public void RecoverEventData(LevelEvent_Base levelEvent)
        {
            if (!expandedEventsAliasData.TryGetValue(levelEvent, out var aliases)
                || !ExpressionCapableTypes.TryGetValue(levelEvent.GetType(), out var list))
            {
                return;
            }

            foreach (var expressionCapable in list)
            {
                if (aliases.TryGetValue(expressionCapable.Name, out var value))
                {
                    expressionCapable.PropertyInfo.SetValue(levelEvent, value);
                }
            }
        }

        public void RecoverConditionalData(Conditional conditional)
        {
            if (conditional is not Conditional_Custom customConditional
                || !expandedCustomConditionalAliasData.TryGetValue(customConditional.id, out var data))
            {
                return;
            }

            customConditional.customExpression = data;
        }

        public bool TryConstructEventJSONData(LevelEvent_Base levelEvent, out string data)
        {
            if (expandedEventsAliasData.TryGetValue(levelEvent, out var aliases))
            {
                StringBuilder builder = new();

                foreach (var alias in aliases)
                {
                    builder.Append($", \"{AliasDynamicKeyPrefix}{alias.Key}\": ");

                    if (alias.Value is FloatExpression floatExpression)
                    {
                        builder.Append(floatExpression.ToJsonValue());
                    }
                    else if (alias.Value is FloatExpression2 floatExpression2)
                    {
                        builder.Append($"[{floatExpression2.x.ToJsonValue()}, {floatExpression2.y.ToJsonValue()}]");
                    }
                    else if (alias.Value is string text)
                    {
                        builder.Append($"\"{text.WithEscapedQuotes()}\"");
                    }
                    else
                    {
                        builder.Append($"\"{alias.Value}\"");
                    }
                }

                data = builder.ToString();
                return true;
            }

            data = default;
            return false;
        }

        public bool TryConstructConditionalJSONData(Conditional conditional, out string data)
        {
            if (conditional is not Conditional_Custom customConditional
                || !expandedCustomConditionalAliasData.TryGetValue(customConditional.id, out string original))
            {
                data = default;
                return false;
            }

            data = $", \"{AliasCustomConditionalExpression}\": \"{original}\"";
            return true;
        }

        static VariableAliasManager()
        {
            var baseType = typeof(LevelEvent_Base);
            var types = baseType.Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));

            foreach (var type in types)
            {
                bool isExpressionEvent = type == typeof(LevelEvent_CallCustomMethod);
                List<ExpressionCapable> list = null;

                foreach (var property in type.GetProperties())
                {
                    var underlyingType = property.PropertyType;
                    if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        underlyingType = Nullable.GetUnderlyingType(underlyingType);
                    }

                    UnderlyingType result;
                    if (underlyingType == typeof(string))
                    {
                        result = isExpressionEvent ? UnderlyingType.ExpressionString : UnderlyingType.String;
                    }
                    else if (underlyingType == typeof(FloatExpression))
                    {
                        result = UnderlyingType.FloatExpression;
                    }
                    else if (underlyingType == typeof(FloatExpression2))
                    {
                        result = UnderlyingType.FloatExpression2;
                    }
                    else
                    {
                        continue;
                    }

                    if (property.GetCustomAttribute<JsonPropertyAttribute>() != null)
                    {
                        (list ??= []).Add(ExpressionCapable.GetExpressionCapableFor(result, property));
                    }
                }

                if (list != null)
                {
                    ExpressionCapableTypes.Add(type, list);
                }
            }
        }

        public enum FailureReason
        {
            AliasNameIsWrong,
            AliasIsBoolean,
            AliasIsFieldOrMethod
        }

        public record DisplayAliasData(string Alias, string Expression);

        public Action OnDataRefresh;

        public const string AliasKey = "variableAlias";

        private void AddToEventAliasData(LevelEvent_Base levelEvent, string key, object value)
        {
            if (expandedEventsAliasData.TryGetValue(levelEvent, out var dict))
            {
                dict.Add(key, value);
                return;
            }

            dict = [];
            dict.Add(key, value);
            expandedEventsAliasData.Add(levelEvent, dict);
        }

        private void UpdateAliasExpansionsFrom(int index)
        {
            for (int i = index; i < aliasData.Count; i++)
            {
                aliasData[i].UpdateExpandedExpression();
            }
        }

        private bool TryExpandAliases(string expression, out string result, int highestIndexAllowed = int.MaxValue)
        {
            StringBuilder reconstructor = null;

            var matches = VariableTokenLookup.Matches(expression);

            if (matches.Count == 0)
            {
                result = expression;
                return false;
            }

            int maxIndex = Math.Min(highestIndexAllowed, aliasData.Count - 1);

            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                if (expression.IsInsideStringLiteral(match.Index))
                {
                    continue;
                }

                for (int j = 0; j <= maxIndex; j++)
                {
                    var data = aliasData[j];

                    if (data.Alias == match.Value)
                    {
                        (reconstructor ??= new(expression))
                            .Remove(match.Index, match.Length)
                            .Insert(match.Index, data.ExpandedExpression);

                        break;
                    }
                }
            }

            if (reconstructor != null)
            {
                result = reconstructor.ToString();
                return true;
            }

            result = expression;
            return false;
        }

        private string ExpandAliases(string text, int highestIndexAllowed = int.MaxValue)
        {
            TryExpandAliases(text, out string result, highestIndexAllowed);
            return result;
        }

        private readonly List<AliasData> aliasData = [];
        private readonly Dictionary<LevelEvent_Base, Dictionary<string, object>> expandedEventsAliasData = [];
        private readonly Dictionary<int, string> expandedCustomConditionalAliasData = [];

        private static VariableAliasManager instance;

        private readonly static Regex AliasNameFormat = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
        private readonly static Regex VariableTokenLookup = new(@"[a-zA-Z_][a-zA-Z0-9_]*");
        private readonly static Regex CurlyBraceFormat = new(@"(?:{([^{}])+})");

        private readonly static Dictionary<Type, List<ExpressionCapable>> ExpressionCapableTypes = [];

        private class ExpressionCapableString(PropertyInfo propertyInfo, bool onlyInBraces) : ExpressionCapable(propertyInfo)
        {
            public override void ExpandAliases(VariableAliasManager manager, LevelEvent_Base levelEvent)
            {
                if (PropertyInfo.GetValue(levelEvent) is not string expression)
                {
                    return;
                }

                var expanded = manager.ExpandAliases(expression, onlyInBraces);

                if (expression != expanded)
                {
                    manager.AddToEventAliasData(levelEvent, PropertyInfo.Name, expression);
                    PropertyInfo.SetValue(levelEvent, expanded);
                }
            }
        }

        private class ExpressionCapableFloatExpression(PropertyInfo propertyInfo) : ExpressionCapable(propertyInfo)
        {
            public override void ExpandAliases(VariableAliasManager manager, LevelEvent_Base levelEvent)
            {
                if (PropertyInfo.GetValue(levelEvent) is not FloatExpression floatExpression
                    || !floatExpression.isExpression)
                {
                    return;
                }

                var expanded = manager.ExpandAliases(floatExpression.exp, onlyInBraces: false);

                if (floatExpression.exp != expanded)
                {
                    manager.AddToEventAliasData(levelEvent, PropertyInfo.Name, floatExpression);
                    PropertyInfo.SetValue(levelEvent, new FloatExpression(expanded));
                }
            }
        }

        private class ExpressionCapableFloatExpression2(PropertyInfo propertyInfo) : ExpressionCapable(propertyInfo)
        {
            public override void ExpandAliases(VariableAliasManager manager, LevelEvent_Base levelEvent)
            {
                if (PropertyInfo.GetValue(levelEvent) is not FloatExpression2 floatExpression2)
                {
                    return;
                }

                FloatExpression x = Expand(manager, floatExpression2.x);
                FloatExpression y = Expand(manager, floatExpression2.y);

                if (!x.Equal(floatExpression2.x) || !y.Equal(floatExpression2.y))
                {
                    manager.AddToEventAliasData(levelEvent, PropertyInfo.Name, floatExpression2);
                    PropertyInfo.SetValue(levelEvent, new FloatExpression2(x, y));
                }
            }

            private static FloatExpression Expand(VariableAliasManager manager, FloatExpression floatExpression)
            {
                if (!floatExpression.isExpression)
                {
                    return floatExpression;
                }

                return new FloatExpression(manager.ExpandAliases(floatExpression.exp, onlyInBraces: false));
            }
        }

        private class ExpressionCapableDummy(PropertyInfo propertyInfo) : ExpressionCapable(propertyInfo)
        {
            public override void ExpandAliases(VariableAliasManager manager, LevelEvent_Base levelEvent) { }
        }

        private abstract class ExpressionCapable(PropertyInfo propertyInfo)
        {
            public static ExpressionCapable GetExpressionCapableFor(UnderlyingType underlyingType, PropertyInfo propertyInfo)
            {
                return underlyingType switch
                {
                    UnderlyingType.String => new ExpressionCapableString(propertyInfo, onlyInBraces: true),
                    UnderlyingType.ExpressionString => new ExpressionCapableString(propertyInfo, onlyInBraces: false),
                    UnderlyingType.FloatExpression => new ExpressionCapableFloatExpression(propertyInfo),
                    UnderlyingType.FloatExpression2 => new ExpressionCapableFloatExpression2(propertyInfo),
                    _ => new ExpressionCapableDummy(propertyInfo)
                };
            }

            public string Name { get; init; } = propertyInfo.Name;
            public PropertyInfo PropertyInfo { get; init; } = propertyInfo;

            public abstract void ExpandAliases(VariableAliasManager manager, LevelEvent_Base levelEvent);
        }

        private enum UnderlyingType
        {
            None,
            String,
            ExpressionString,
            FloatExpression,
            FloatExpression2
        }

        private const string AliasDynamicKeyPrefix = "mod_rdEditorPlus_alias_";
        private const string AliasCustomConditionalExpression = "mod_rdEditorPlus_alias_expression";

        private record ReplacementValues(string KeyPart, object Value);

        private class AliasData
        {
            public AliasData(VariableAliasManager manager, string alias, string expression, int index)
            {
                this.manager = manager;
                this.expression = expression;
                Alias = alias;
                Index = index;

                UpdateExpandedExpression();
            }

            public string Alias { get; set; }
            public string Expression
            {
                get => expression;
                set
                {
                    expression = value;
                    UpdateExpandedExpression();
                }
            }
            public int Index { get; set; }
            public string ExpandedExpression { get; private set; }

            public void UpdateExpandedExpression()
            {
                ExpandedExpression = manager.ExpandAliases(IntermediaryExpression, Index - 1);
                Plugin.LogInfo($"Updated expansion for {Alias}: {Expression} -> {ExpandedExpression}");
            }

            public void SwapWith(AliasData other)
            {
                (expression, other.expression) = (other.expression, expression);
                (Alias, other.Alias) = (other.Alias, Alias);
            }

            private string IntermediaryExpression => WrapInParenthesisIfRequired(expression.EvaluateScientificLiterals());

            private string expression;
            private readonly VariableAliasManager manager;

            private string WrapInParenthesisIfRequired(string text)
            {
                string trimmed = text.Trim();

                if (trimmed.IsNullOrEmpty()
                    || trimmed.IsExpressionWrappedInParenthesis()
                    || bool.TryParse(trimmed, out _) || float.TryParse(trimmed, out _)
                    || manager.AliasExists(trimmed) || typeof(LevelBase).GetField(trimmed) != null)
                {
                    return text;
                }

                return $"({text})";
            }
        }
    }
}
