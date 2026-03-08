using RDEditorPlus.Functionality.NodeEditor.Nodes;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeEditor.Grid
{
    public class NodeGridView : MonoBehaviour
    {
        public static NodeGridView Create(Transform parent, Sprite sprite, NodePanelHolder holder)
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

            rectTransform.anchorMin = new Vector2(0.02f, 0.18f);
            rectTransform.anchorMax = new Vector2(0.98f, 0.88f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = new Vector2(0f, -10f);

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

            component.grid = NodeGrid.Create(maskRectTransform, holder);
            component.sprite = sprite;

            view.SetActive(true);
            return component;
        }

        public Node AddNode(GameObject prefab, Vector2 position, string id) => grid.AddNode(prefab, position, id);

        public void Reset()
        {
            grid.Reset();
        }

        public async Task SaveAsync(XmlWriter writer)
        {
            await grid.SaveAsync(writer);
        }

        public void Clear() => grid.Clear();

        public bool TryGetNodeFromID(string id, out Node result) => grid.TryGetNodeFromID(id, out result);

        public Node[] GetDependencyOrderedNodes() => grid.GetDependencyOrderedNodes();

        private NodeGrid grid;
        private Sprite sprite;
    }
}
