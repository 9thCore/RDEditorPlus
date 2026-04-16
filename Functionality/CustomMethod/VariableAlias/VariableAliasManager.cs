using HarmonyLib;
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

        public void UpdateAliasDataLength()
        {
            aliasDataLongestToShortest = aliasData.OrderByDescending(alias => alias.Alias.Length).ToArray();
        }

        public string ExpandAliases(string expression, bool onlyInBraces)
        {
            if (onlyInBraces)
            {
                var matches = CurlyBraceFormat.Matches(expression);

                Plugin.LogInfo(expression);
                foreach (Match match in matches)
                {
                    Plugin.LogWarn(match.Value);
                }

                return expression;
            }
            else
            {
                StringBuilder builder = new(expression);

                foreach (var alias in aliasDataLongestToShortest)
                {
                    builder.Replace(alias.Alias, alias.ExpandedExpression);
                }

                return builder.ToString();
            }
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
        public void ClearOriginalValues() => expandedEventsAliasData.Clear();
        public void Remove(int index)
        {
            aliasData.RemoveAt(index);
            UpdateAliasDataLength();
        }

        public void SetAlias(int index, string alias)
        {
            aliasData[index].SetAlias(alias);
            UpdateAliasDataLength();
        }

        public void SetExpression(int index, string expression)
        {
            aliasData[index].SetExpression(expression);
        }

        public bool AliasExists(string alias) => aliasData.Any(data => data.Alias == alias);

        public void CreateAlias(string alias, string expression)
        {
            if (AliasExists(alias))
            {
                return;
            }

            StringBuilder parser = new(expression);

            // This conversion is needed because RDCode does not support scientific literals
            var matches = ScientificLiteralFormat.Matches(expression);
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                if (float.TryParse(matches[i].Value, out var floatValue))
                {
                    parser
                        .Remove(matches[i].Index, matches[i].Length)
                        .Insert(matches[i].Index, floatValue.AsRawValue());
                }
            }

            aliasData.Add(new AliasData(alias, parser.ToString()));
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
                UpdateAliasDataLength();
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

            UpdateAliasDataLength();
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

        private AliasData[] aliasDataLongestToShortest = [];

        private readonly List<AliasData> aliasData = [];
        private readonly Dictionary<LevelEvent_Base, Dictionary<string, object>> expandedEventsAliasData = [];

        private static VariableAliasManager instance;

        private readonly static Regex ScientificLiteralFormat = new(@"\-?\d+(?:\.\d+)?e[\-\+]?\d+");
        private readonly static Regex AliasNameFormat = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
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

        private record ReplacementValues(string KeyPart, object Value);

        private class AliasData(string key, string expression)
        {
            public string Alias { get; private set; } = key;
            public string Expression { get; private set; } = expression;

            public string ExpandedExpression { get; private set; } = expression;

            public void Expand()
            {
                ExpandedExpression = Expression;
            }

            public void SetAlias(string alias) => Alias = alias;
            public void SetExpression(string expression)
            {

            }
        }
    }
}
