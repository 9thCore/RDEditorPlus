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

        public static Vector2 GetCenterFromCorners(this Vector3[] worldCorners)
        {
            Vector3 topLeft = worldCorners[2];
            Vector3 bottomRight = worldCorners[0];

            return new Vector2((topLeft.x + bottomRight.x) / 2f, (topLeft.y + bottomRight.y) / 2f);
        }
    }
}
