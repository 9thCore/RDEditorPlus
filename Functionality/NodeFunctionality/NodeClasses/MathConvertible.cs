using nn.util;
using RDEditorPlus.Util;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct MathConvertible
    {
        public object Value { get; private init; }

        public override string ToString()
        {
            return $"{Value} ({Value.GetType()})";
        }

        public MathConvertible(MathConvertible other) : this(other.type, other.Value)
        {
            intValue = other.intValue;
            floatValue = other.floatValue;

            float2Value.x = other.float2Value.x;
            float2Value.y = other.float2Value.y;
            float2Value.xUsed = other.float2Value.xUsed;
            float2Value.yUsed = other.float2Value.yUsed;
        }

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

            return value;
        }

        public static MathConvertible operator+(MathConvertible left, MathConvertible right)
        {
            return left.type switch
            {
                Type.Int => right.type switch
                {
                    Type.Int => new(left.intValue + right.intValue),
                    Type.Float => new(left.intValue + right.floatValue),
                    Type.Float2 => new(
                        left.intValue + right.float2Value.x, left.intValue + right.float2Value.y,
                        right.float2Value.xUsed, right.float2Value.yUsed),
                    _ => left
                },
                Type.Float => right.type switch
                {
                    Type.Int => new(left.floatValue + right.intValue),
                    Type.Float => new(left.floatValue + right.floatValue),
                    Type.Float2 => new(
                        left.floatValue + right.float2Value.x, left.floatValue + right.float2Value.y,
                        right.float2Value.xUsed, right.float2Value.yUsed),
                    _ => left
                },
                Type.Float2 => right.type switch
                {
                    Type.Int => new(
                        left.float2Value.x + right.intValue, left.float2Value.y + right.intValue,
                        left.float2Value.xUsed, left.float2Value.yUsed),
                    Type.Float => new(
                        left.float2Value.x + right.floatValue, left.float2Value.y + right.floatValue,
                        left.float2Value.xUsed, left.float2Value.yUsed),
                    Type.Float2 => new(
                        left.float2Value.XOr(0) + right.float2Value.XOr(0), left.float2Value.YOr(0) + right.float2Value.YOr(0),
                        left.float2Value.xUsed || right.float2Value.xUsed, left.float2Value.yUsed || right.float2Value.yUsed),
                    _ => left
                },
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

        private MathConvertible(float x, float y, bool xUsed, bool yUsed)
            : this(new Float2(x, y) { xUsed = xUsed, yUsed = yUsed }) { }

        private MathConvertible(Type type, object value)
        {
            this.type = type;
            Value = value;
        }

        private readonly int intValue;
        private readonly float floatValue;
        private readonly Float2 float2Value;
        private readonly Type type;

        private enum Type
        {
            Int,
            Float,
            Float2
        }
    }

    //public readonly struct MathConvertible
    //{
    //    public readonly override string ToString()
    //    {
    //        return value.ToString();
    //    }

    //    // The integer type is the "least powerful" of them all, so it's best fit for a default value
    //    public MathConvertible() : this(new IntegerValue(0)) { }

    //    public object Value => value.UnderlyingValue;

    //    public static MathConvertible operator +(MathConvertible a, MathConvertible b) => new(a.value.Add(b.value));

    //    public static object Convert(object value)
    //    {
    //        if (value is int integer)
    //        {
    //            return new MathConvertible(new IntegerValue(integer));
    //        }
    //        else if (value is float single)
    //        {
    //            return new MathConvertible(new SingleValue(single));
    //        }
    //        else if (value is Float2 float2)
    //        {
    //            return new MathConvertible(new Float2Value(float2));
    //        }

    //        return value;
    //    }

    //    private MathConvertible(IValue value)
    //    {
    //        this.value = value;
    //    }

    //    private readonly IValue value;

    //    private readonly record struct SingleValue(float Value) : IValue
    //    {
    //        public object UnderlyingValue => Value;

    //        public IValue Add(IValue secondary)
    //        {
    //            if (secondary is SingleValue single)
    //            {
    //                return new SingleValue(Value + single.Value);
    //            }
    //            else if (secondary is IntegerValue integer)
    //            {
    //                return new SingleValue(Value + integer.Value);
    //            }
    //            else if (secondary is Float2Value float2)
    //            {
    //                return new Float2Value(Value + float2.Value.x, Value + float2.Value.y);
    //            }

    //            return default;
    //        }
    //    }

    //    private readonly record struct IntegerValue(int Value) : IValue
    //    {
    //        public object UnderlyingValue => Value;

    //        public IValue Add(IValue secondary)
    //        {
    //            if (secondary is SingleValue single)
    //            {
    //                return new SingleValue(Value + single.Value);
    //            }
    //            else if (secondary is IntegerValue integer)
    //            {
    //                return new IntegerValue(Value + integer.Value);
    //            }
    //            else if (secondary is Float2Value float2)
    //            {
    //                return new Float2Value(Value + float2.Value.x, Value + float2.Value.y);
    //            }

    //            return default;
    //        }
    //    }

    //    private readonly record struct Float2Value(Float2 Value) : IValue
    //    {
    //        public Float2Value(float x, float y) : this(new(x, y)) { }

    //        public object UnderlyingValue => Value;

    //        public IValue Add(IValue secondary)
    //        {
    //            if (secondary is SingleValue single)
    //            {
    //                return new Float2Value(single.Value + Value.x, single.Value + Value.y);
    //            }
    //            else if (secondary is IntegerValue integer)
    //            {
    //                return new Float2Value(integer.Value + Value.x, integer.Value + Value.y);
    //            }
    //            else if (secondary is Float2Value float2)
    //            {
    //                return new Float2Value(Value + float2.Value);
    //            }

    //            return default;
    //        }
    //    }

    //    private interface IValue
    //    {
    //        object UnderlyingValue { get; }
    //        IValue Add(IValue secondary);
    //    }
    //}
}
