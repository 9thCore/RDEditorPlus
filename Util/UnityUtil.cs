using UnityEngine;
using UnityEngine.UI;

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

        public static void OffsetMinRelativeY(this RectTransform transform, float delta)
        {
            transform.OffsetMinY(transform.offsetMin.y + delta);
        }

        public static void OffsetMaxY(this RectTransform transform, float value)
        {
            transform.offsetMax = new Vector2(transform.offsetMax.x, value);
        }

        public static T ReplaceWithDerivative<T>(this Dropdown source) where T : Dropdown
        {
            GameObject gameObject = source.gameObject;
            bool editorDropdownWasPresent = false;

            var properties = (source.template, source.alphaFadeSpeed, source.animationTriggers, source.captionImage, source.captionText,
                source.colors, source.image, source.interactable, source.itemImage, source.itemText, source.onValueChanged,
                 source.options, source.value);

            if (gameObject.TryGetComponent(out EditorDropdown component))
            {
                editorDropdownWasPresent = true;
                GameObject.DestroyImmediate(component);
            }

            GameObject.DestroyImmediate(source);

            T destination = gameObject.AddComponent<T>();

            if (editorDropdownWasPresent)
            {
                gameObject.AddComponent<EditorDropdown>();
            }

            (destination.template, destination.alphaFadeSpeed, destination.animationTriggers, destination.captionImage,
                destination.captionText, destination.colors, destination.image, destination.interactable,
                destination.itemImage, destination.itemText, destination.onValueChanged,destination.options, destination.value) = properties;

            return destination;
        }
    }
}
