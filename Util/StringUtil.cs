namespace RDEditorPlus.Util
{
    public static class StringUtil
    {
        public static string AsRawValue(this float value) => value.ToString(RawValueFormat);

        private static readonly string RawValueFormat = "0." + new string('#', 52);
    }
}
