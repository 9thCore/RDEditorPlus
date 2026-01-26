namespace RDEditorPlus.Util
{
    internal static class FloatExpressionUtil
    {
        public static bool Equal(this FloatExpression lhs, FloatExpression rhs)
        {
            if (lhs.isExpression != rhs.isExpression)
            {
                return false;
            }

            if (lhs.isExpression)
            {
                return lhs.exp == rhs.exp;
            }

            return lhs.num == rhs.num;
        }
    }
}
