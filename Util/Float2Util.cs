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
    }
}
