using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeEditor.Grid
{
    public class NodeGridView : MonoBehaviour
    {
        public static NodeGridView Create(Transform parent, Sprite sprite)
        {
            GameObject view = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(NodeGridView)}");
            view.SetActive(false);

            var component = view.AddComponent<NodeGridView>();

            var image = view.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;

            var rectTransform = view.transform as RectTransform;

            rectTransform.SetParent(parent);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;

            rectTransform.anchorMin = new Vector2(0.02f, 0.02f);
            rectTransform.anchorMax = new Vector2(0.98f, 0.88f);
            rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;

            GameObject mask = new("mask");

            var maskImage = mask.AddComponent<Image>();
            maskImage.sprite = sprite;
            maskImage.type = Image.Type.Sliced;

            mask.AddComponent<Mask>().showMaskGraphic = false;

            var maskRectTransform = mask.transform as RectTransform;

            maskRectTransform.SetParent(rectTransform);
            maskRectTransform.localPosition = Vector3.zero;
            maskRectTransform.localScale = Vector3.one;

            maskRectTransform.anchorMin = new Vector2(0.01f, 0.02f);
            maskRectTransform.anchorMax = new Vector2(0.99f, 0.98f);
            maskRectTransform.offsetMin = maskRectTransform.offsetMax = Vector2.zero;

            component.grid = NodeGrid.Create(maskRectTransform);
            component.sprite = sprite;

            view.SetActive(true);
            return component;
        }

        public void AddNode(GameObject prefab, Vector2 position) => grid.AddNode(prefab, position);

        public void Reset()
        {
            grid.Reset();
        }

        private NodeGrid grid;
        private Sprite sprite;
    }
}
