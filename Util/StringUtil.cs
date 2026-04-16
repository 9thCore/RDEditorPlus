using System.Text;
using System.Text.RegularExpressions;

namespace RDEditorPlus.Util
{
    public static class StringUtil
    {
        public static bool IsExpressionWrappedInParenthesis(this string text)
        {
            if (text.Length == 0)
            {
                return false;
            }

            if (text[0] != '(' || text[text.Length - 1] != ')')
            {
                return false;
            }

            int runningTotal = 1;
            for (int i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == '(') runningTotal++;
                else if (text[i] == ')') runningTotal--;

                if (runningTotal == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static string EvaluateScientificLiterals(this string text)
        {
            StringBuilder parser = null;

            var matches = ScientificLiteralFormat.Matches(text);
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                if (IsActualStartOfScientificLiteral(text, matches[i].Index)
                    && float.TryParse(matches[i].Value, out var floatValue))
                {
                    (parser ??= new(text))
                        .Remove(matches[i].Index, matches[i].Length)
                        .Insert(matches[i].Index, floatValue.AsRawValue());
                }
            }

            return parser == null ? text : parser.ToString();
        }

        public static string AsRawValue(this float value) => value.ToString(RawValueFormat);

        private static bool IsActualStartOfScientificLiteral(string text, int index)
        {
            if (index == 0)
            {
                return true;
            }

            return !char.IsLetter(text[index - 1]);
        }

        private static readonly string RawValueFormat = "0." + new string('#', 52);
        private static readonly Regex ScientificLiteralFormat = new(@"\-?\d+(?:\.\d+)?e[\-\+]?\d+");
    }
}
