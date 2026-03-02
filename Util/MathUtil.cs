using UnityEngine;

namespace RDEditorPlus.Util
{
    public static class MathUtil
    {
        public static Vector2 Floor(this Vector2 vector)
        {
            return new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
        }

        public static Vector2 RoundToMultiple(this Vector2 vector, float value)
        {
            return (vector / value).Floor() * value;
        }
    }
}
