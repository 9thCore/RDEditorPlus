namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct MathConvertible
    {
        public readonly override string ToString()
        {
            return value.ToString();
        }

        // The integer type is the "least powerful" of them all, so it's best fit for a default value
        public MathConvertible() : this(new IntegerValue(0)) { }

        public object Value => value.UnderlyingValue;

        public static MathConvertible operator +(MathConvertible a, MathConvertible b) => new(a.value.Add(b.value));

        public static object Convert(object value)
        {
            if (value is int integer)
            {
                return new MathConvertible(new IntegerValue(integer));
            }
            else if (value is float single)
            {
                return new MathConvertible(new SingleValue(single));
            }

            return value;
        }

        private MathConvertible(IValue value)
        {
            this.value = value;
        }

        private readonly IValue value;

        private readonly record struct SingleValue(float Value) : IValue
        {
            public object UnderlyingValue => Value;

            public IValue Add(IValue secondary)
            {
                if (secondary is SingleValue single)
                {
                    return new SingleValue(Value + single.Value);
                }
                else if (secondary is IntegerValue integer)
                {
                    return new SingleValue(Value + integer.Value);
                }

                return default;
            }
        }

        private readonly record struct IntegerValue(int Value) : IValue
        {
            public object UnderlyingValue => Value;

            public IValue Add(IValue secondary)
            {
                if (secondary is SingleValue single)
                {
                    return new SingleValue(Value + single.Value);
                }
                else if (secondary is IntegerValue integer)
                {
                    return new IntegerValue(Value + integer.Value);
                }

                return default;
            }
        }

        private interface IValue
        {
            object UnderlyingValue { get; }
            IValue Add(IValue secondary);
        }
    }
}
