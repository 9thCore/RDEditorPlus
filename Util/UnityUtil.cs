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

        public static T ReplaceWithDerivative<T>(this Button source) where T : Button
        {
            GameObject gameObject = source.gameObject;
            var properties = (source.colors);

            GameObject.DestroyImmediate(source);
            T destination = gameObject.AddComponent<T>();

            (destination.colors) = properties;
            return destination;
        }

        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            graphic.color = graphic.color.WithAlpha(alpha);
        }

        public static void SetNormalizedValueWithoutNotify(this Slider slider, float normalizedValue)
        {
            slider.SetValueWithoutNotify(Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue));
        }

        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T result) where T : Component
        {
            result = gameObject.GetComponentInParent<T>();
            return result != null;
        }

        public static bool TryGetComponentInParent<T>(this Component component, out T result) where T : Component
        {
            result = component.GetComponentInParent<T>();
            return result != null;
        }

        public static void CreateDropdown(Transform parent, out Dropdown dropdown, out RectTransform dropdownRT)
        {
            var controlInstance = GameObject.Instantiate(RDConstants.data.dropdownControlPrefab);

            dropdown = controlInstance.GetComponentInChildren<Dropdown>();

            dropdownRT = dropdown.transform as RectTransform;
            dropdownRT.SetParent(parent);
            dropdownRT.localPosition = Vector3.zero;
            dropdownRT.localScale = Vector3.one;

            GameObject.DestroyImmediate(dropdown.GetComponent<EditorDropdown>());
            GameObject.DestroyImmediate(controlInstance);
        }

        public static void CreateScrollView(Transform parent, out ScrollRect scrollRect, out RectTransform contentRT)
        {
            GameObject scrollviewGO = new("scrollview");
            scrollRect = scrollviewGO.AddComponent<ScrollRect>();

            var scrollviewRT = scrollviewGO.transform as RectTransform;
            scrollviewRT.SetParent(parent, worldPositionStays: false);
            scrollviewRT.anchorMin = Vector2.zero;
            scrollviewRT.anchorMax = Vector2.one;
            scrollviewRT.offsetMin = new Vector2(4f, 10f);
            scrollviewRT.offsetMax = new Vector2(-4f, -20f);

            CreateScrollViewport(scrollviewRT, out contentRT, out var viewportRT);
            CreateScrollbar(scrollviewRT, out var scrollbar);

            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.horizontal = false;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            scrollbar.direction = Scrollbar.Direction.BottomToTop;
        }

        private static void CreateScrollViewport(Transform parent, out RectTransform contentRT, out RectTransform viewportRT)
        {
            GameObject viewportGO = new("viewport");

            var viewportImage = viewportGO.AddComponent<Image>();
            viewportImage.sprite = AssetUtil.InputFieldSprite;
            viewportImage.type = Image.Type.Tiled;
            viewportImage.color = Color.Lerp(Color.gray, Color.black, 0.65f);

            viewportGO.AddComponent<Mask>();

            viewportRT = viewportGO.transform as RectTransform;
            viewportRT.SetParent(parent, worldPositionStays: false);
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;

            GameObject contentGO = new("content");

            contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.SetParent(viewportRT, worldPositionStays: false);
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;
        }

        private static void CreateScrollbar(Transform parent, out Scrollbar scrollbar)
        {
            GameObject scrollbarGO = new("scrollbar");
            scrollbar = scrollbarGO.AddComponent<Scrollbar>();

            var scrollbarImage = scrollbarGO.AddComponent<Image>();
            scrollbarImage.enabled = false;

            var scrollbarRT = scrollbarGO.transform as RectTransform;
            scrollbarRT.SetParent(parent, worldPositionStays: false);
            scrollbarRT.anchorMin = new Vector2(1f, 0f);
            scrollbarRT.anchorMax = Vector2.one;
            scrollbarRT.offsetMin = new Vector2(-10f, 5f);
            scrollbarRT.offsetMax = new Vector2(-5f, -5f);

            GameObject slidingAreaGO = new("slidingArea");

            var slidingAreaImage = slidingAreaGO.AddComponent<Image>();
            slidingAreaImage.sprite = AssetUtil.InputFieldSprite;
            slidingAreaImage.type = Image.Type.Tiled;
            slidingAreaImage.color = Color.Lerp(Color.gray, Color.black, 0.75f);

            var slidingAreaRT = slidingAreaGO.transform as RectTransform;
            slidingAreaRT.SetParent(scrollbarRT, worldPositionStays: false);
            slidingAreaRT.anchorMin = Vector2.zero;
            slidingAreaRT.anchorMax = Vector2.one;
            slidingAreaRT.offsetMin = Vector2.zero;
            slidingAreaRT.offsetMax = Vector2.zero;

            GameObject handleGO = new("handle");

            var handleImage = handleGO.AddComponent<Image>();
            handleImage.sprite = AssetUtil.ButtonSprite;
            handleImage.type = Image.Type.Tiled;

            var handleRT = handleGO.transform as RectTransform;
            handleRT.SetParent(slidingAreaRT, worldPositionStays: false);
            handleRT.offsetMin = Vector2.zero;
            handleRT.offsetMax = Vector2.zero;

            scrollbar.handleRect = handleRT;
            scrollbar.targetGraphic = handleImage;
            scrollbar.image = scrollbarImage;
        }
    }
}
