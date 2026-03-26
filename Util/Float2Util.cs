namespace RDEditorPlus.Util
{
    internal static class Float2Util
    {
        public static Float2 Combine(Float2 xProvider, Float2 yProvider)
        {
            return new Float2
            {
                x = xProvider.x,
                y = yProvider.y,
                xUsed = xProvider.xUsed,
                yUsed = yProvider.yUsed
            };
        }

        public static float X(this Float2 float2)
        {
            return float2.xUsed ? float2.x : float.NaN;
        }

        public static float Y(this Float2 float2)
        {
            return float2.yUsed ? float2.y : float.NaN;
        }

        public static float XOr(this Float2 float2, float defaultValue) => float2.xUsed ? float2.x : defaultValue;
        public static float YOr(this Float2 float2, float defaultValue) => float2.yUsed ? float2.y : defaultValue;

        public static Float2 ParseSerialised(string text)
        {
            Float2 result = new(0f, 0f);
            int index = text.IndexOf(SerialisationSeparator);

            if (index < 0)
            {
                result.yUsed = false;

                if (!float.TryParse(text, out result.x))
                {
                    result.xUsed = false;
                }

                return result;
            }

            string xText = text.Substring(0, index);
            string yText = text.Substring(index + 1);

            if (!float.TryParse(xText, out result.x))
            {
                result.xUsed = false;
            }

            if (!float.TryParse(yText, out result.y))
            {
                result.yUsed = false;
            }

            return result;
        }

        public const char SerialisationSeparator = '|';
    }
}
