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
    }
}
