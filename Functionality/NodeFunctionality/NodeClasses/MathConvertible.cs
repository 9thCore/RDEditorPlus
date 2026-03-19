using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using System;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct MathConvertible
    {
        public object Value { get; private init; }

        public override string ToString()
        {
            return $"{Value} ({Value.GetType()})";
        }

        public MathConvertible() : this(hasValue: false) { }

        public static object Convert(object value)
        {
            if (value is int intValue)
            {
                return new MathConvertible(intValue);
            }
            else if (value is float floatValue)
            {
                return new MathConvertible(floatValue);
            }
            else if (value is Float2 float2Value)
            {
                return new MathConvertible(float2Value);
            }
            else if (value is FloatExpression floatExpressionValue)
            {
                return new MathConvertible(floatExpressionValue);
            }
            else if (value is FloatExpression2 floatExpression2Value)
            {
                return new MathConvertible(floatExpression2Value);
            }

            return value;
        }

        public static MathConvertible operator+(MathConvertible left, MathConvertible right)
        {
            if (left.type == Type.Int && right.type == Type.Int)
            {
                return new(left.intValue + right.intValue);
            }

            var leftData = left.Data;
            var rightData = right.Data;

            Type bestFit = GetBestFitFor(left.type, right.type);

            return bestFit switch
            {
                Type.Float => new MathConvertible((leftData.Simple + rightData.Simple).Value),
                Type.FloatExpression => new MathConvertible((leftData.Simple + rightData.Simple).Expression),
                Type.Float2 => new MathConvertible(
                    (leftData.X + rightData.X).Value, (leftData.Y + rightData.Y).Value,
                    leftData.X.ForceInMultiValue || rightData.X.ForceInMultiValue, leftData.Y.ForceInMultiValue || rightData.Y.ForceInMultiValue),
                Type.FloatExpression2 => new MathConvertible(
                    (leftData.X.ForceInMultiValue || rightData.X.ForceInMultiValue) ? (leftData.X + rightData.X).Expression : string.Empty,
                    (leftData.Y.ForceInMultiValue || rightData.Y.ForceInMultiValue) ? (leftData.Y + rightData.Y).Expression : string.Empty),
                _ => left
            };
        }

        private MathConvertible(int value) : this(Type.Int, value)
        {
            intValue = value;
        }

        private MathConvertible(float value) : this(Type.Float, value)
        {
            floatValue = value;
        }

        private MathConvertible(Float2 value) : this(Type.Float2, value)
        {
            float2Value = value;
        }

        private MathConvertible(FloatExpression value) : this(Type.FloatExpression, value)
        {
            floatExpressionValue = value;
        }

        private MathConvertible(FloatExpression2 value) : this(Type.FloatExpression2, value)
        {
            floatExpression2Value = value;
        }

        private MathConvertible(float x, float y, bool xUsed, bool yUsed)
            : this(new Float2(x, y) { xUsed = xUsed, yUsed = yUsed }) { }

        private MathConvertible(string expression)
            : this(new FloatExpression(expression)) { }

        private MathConvertible(string xExpression, string yExpression)
            : this(new FloatExpression2(new FloatExpression(xExpression), new FloatExpression(yExpression))) { }

        private MathConvertible(Type type, object value) : this(hasValue: true)
        {
            this.type = type;
            Value = value;
        }

        private MathConvertible(bool hasValue)
        {
            this.hasValue = hasValue;
        }

        private ConvertibleData Data
        {
            get
            {
                if (!hasValue)
                {
                    return new(new AxisData(), new AxisData());
                }

                (AxisData x, AxisData y) = type switch
                {
                    Type.Int => (intValue, intValue),
                    Type.Float => (floatValue, floatValue),
                    Type.FloatExpression => (floatExpressionValue, floatExpressionValue),
                    Type.Float2 => (AxisData.FromFloat2(float2Value.x, float2Value.xUsed), AxisData.FromFloat2(float2Value.y, float2Value.yUsed)),
                    Type.FloatExpression2 => (AxisData.FromExpression2(floatExpression2Value.x), AxisData.FromExpression2(floatExpression2Value.y)),
                    _ => (new AxisData(), new AxisData())
                };

                return new(x, y);
            }
        }

        private readonly int intValue;
        private readonly float floatValue;
        private readonly FloatExpression floatExpressionValue;
        private readonly Float2 float2Value;
        private readonly FloatExpression2 floatExpression2Value;
        private readonly Type type;
        private readonly bool hasValue;

        private readonly record struct AxisData(float Value, string Expression, AxisData.DataType Type, bool ForceInMultiValue)
        {
            public AxisData(float value, bool forceInMultiValue) : this(value, string.Empty, DataType.Value, forceInMultiValue) { }
            public AxisData(float value) : this(value, forceInMultiValue: false) { }

            public AxisData(string expression, bool forceInMultiValue) : this(0f, expression, DataType.Expression, forceInMultiValue) { }
            public AxisData(string expression) : this(expression, forceInMultiValue: false) { }

            public AxisData() : this(0f, string.Empty, DataType.None, ForceInMultiValue: false) { }

            public override string ToString()
                => Type switch
                {
                    DataType.None => string.Empty,
                    DataType.Expression => Expression,
                    DataType.Value => Value.ToString(),
                    DataType.Float2Value => ForceInMultiValue ? Value.ToString() : string.Empty,
                    _ => string.Empty
                };

            public static AxisData FromFloat2(float value, bool used)
                => new(value, string.Empty, DataType.Float2Value, used);

            public static AxisData FromExpression2(FloatExpression floatExpression)
            {
                if (floatExpression.isExpression)
                {
                    return new(floatExpression.exp, forceInMultiValue: !floatExpression.exp.IsNullOrEmpty());
                }

                return new(floatExpression.num, forceInMultiValue: true);
            }

            public static AxisOperationResult operator +(AxisData left, AxisData right) => new('+', left, right);

            public static implicit operator AxisData(float value) => new(value);
            public static implicit operator AxisData(string expression) => new(expression);
            public static implicit operator AxisData(FloatExpression floatExpression)
                => floatExpression.isExpression ? floatExpression.exp : floatExpression.num;

            public enum DataType
            {
                None,
                Value,
                Float2Value,
                Expression
            }
        }

        private readonly record struct ConvertibleData(AxisData X, AxisData Y)
        {
            /// <summary>
            /// Intended to be used by primitives, where X == Y
            /// </summary>
            public AxisData Simple => X;
        }

        private readonly record struct AxisOperationResult(char Operator, in AxisData Left, in AxisData Right)
        {
            public float Value => Calculate(Left.Value, Right.Value);

            public string Expression
            {
                get
                {
                    var lhs = Left.ToString();
                    var rhs = Right.ToString();

                    if (lhs.IsNullOrEmpty() && rhs.IsNullOrEmpty())
                    {
                        return string.Empty;
                    }
                    else if (lhs.IsNullOrEmpty())
                    {
                        return rhs;
                    }
                    else if (rhs.IsNullOrEmpty())
                    {
                        return lhs;
                    }

                    if (float.TryParse(lhs, out var left)
                        && float.TryParse(rhs, out var right))
                    {
                        return Calculate(left, right).ToString();
                    }

                    return $"({lhs}){Operator}({rhs})";
                }
            }

            private float Calculate(float left, float right)
                => Operator switch
                {
                    '+' => left + right,
                    _ => 0f
                };
        }

        private enum Type
        {
            Int,
            Float,
            FloatExpression,
            Float2,
            FloatExpression2,
        }
        
        /// <summary>
        /// Straight copy from <see cref="Node.GetBestFitFor(Node.Type, Node.Type)"/> but whatever
        /// </summary>
        private static Type GetBestFitFor(Type left, Type right)
        {
            if (left == Type.FloatExpression2 || right == Type.FloatExpression2)
            {
                return Type.FloatExpression2;
            }
            else if ((left == Type.FloatExpression && right == Type.Float2)
                || (left == Type.Float2 && right == Type.FloatExpression))
            {
                return Type.FloatExpression2;
            }

            return (Type)Math.Max((int)left, (int)right);
        }
    }
}
