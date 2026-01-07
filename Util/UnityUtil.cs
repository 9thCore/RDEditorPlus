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

        public static void AnchorRelativeY(this RectTransform transform, float delta)
        {
            transform.AnchorPosY(transform.anchoredPosition.y + delta);
        }
    }
}
