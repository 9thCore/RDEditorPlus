namespace RDEditorPlus.Util
{
    public static class FloatExpression2Util
    {
        public static string Serialise(FloatExpression2 value) => $"{value.x}{SerialisationSeparator}{value.y}";

        public static FloatExpression2 ParseSerialised(string text)
        {
            int index = text.IndexOf(SerialisationSeparator);

            if (index < 0)
            {
                return new(FloatExpression.FromString(text), FloatExpression.EmptyInput());
            }

            string x = text.Substring(0, index);
            string y = text.Substring(index + 1);

            return new(FloatExpression.FromString(x), FloatExpression.FromString(y));
        }

        public const char SerialisationSeparator = '|';
    }
}
