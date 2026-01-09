using UnityEngine;

namespace RDEditorPlus.Util
{
    public static class UnityUtil
    {
        public static C EnsureComponent<C>(this GameObject gameObject) where C : Component
        {
            if (gameObject.TryGetComponent(out C component))
            {
                return component;
            }

            return gameObject.AddComponent<C>();
        }

        public static C EnsureComponent<C>(this Component component) where C : Component
        {
            return EnsureComponent<C>(component.gameObject);
        }

        public static void AnchorRelativeY(this RectTransform transform, float delta)
        {
            transform.AnchorPosY(transform.anchoredPosition.y + delta);
        }

        public static void OffsetMinY(this RectTransform transform, float value)
        {
            transform.offsetMin = new Vector2(transform.offsetMin.x, value);
        }

        public static void OffsetMaxY(this RectTransform transform, float value)
        {
            transform.offsetMax = new Vector2(transform.offsetMax.x, value);
        }
    }
}
