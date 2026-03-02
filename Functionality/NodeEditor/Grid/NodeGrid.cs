using RDEditorPlus.Util;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeEditor.Grid
{
    public class NodeGrid : MonoBehaviour
    {
        public static NodeGrid Create(Transform parent)
        {
            GameObject root = new(nameof(NodeGrid));
            root.SetActive(false);

            root.transform.SetParent(parent);
            root.transform.localScale = Vector3.one;
            root.transform.localPosition = Vector3.zero;

            GameObject background = new("background");

            var image = background.AddComponent<Image>();
            image.sprite = GridSprite;
            image.type = Image.Type.Tiled;

            NodeGrid component = root.AddComponent<NodeGrid>();
            root.AddComponent<NodeGridEventTrigger>().Setup(component);

            var rt = background.transform as RectTransform;
            component.background = rt;
            component.space = root.AddComponent<RectTransform>();

            rt.SetParent(root.transform);
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;

            rt.offsetMin = new Vector2(-300f, -200f);
            rt.offsetMax = -rt.offsetMin;

            root.SetActive(true);
            return component;
        }

        public void AddNode(GameObject prefab, Vector2 position)
        {
            GameObject instance = GameObject.Instantiate(prefab, space);

            RectTransform rt = instance.transform as RectTransform;
            rt.transform.localScale = Vector3.one;
            rt.anchoredPosition = position;

            instance.SetActive(true);
        }

        public void Drag(Vector2 delta)
        {
            if (delta == Vector2.zero)
            {
                return;
            }

            space.anchoredPosition += delta / space.lossyScale;
            UpdateBackground();
        }

        public void Reset()
        {
            space.anchoredPosition = Vector2.zero;
            UpdateBackground();
        }

        private void UpdateBackground()
        {
            background.anchoredPosition = -space.anchoredPosition.RoundToMultiple(GridSize);
        }

        private RectTransform space;
        private RectTransform background;

        public static Sprite GridSprite
        {
            get
            {
                if (gridSprite == null)
                {
                    Texture2D texture = new(GridSize, GridSize);
                    Color main = Color.gray;
                    Color background = Color.black;

                    for (int i = 0; i < GridSize; i++)
                    {
                        for (int j = 0; j < GridSize; j++)
                        {
                            texture.SetPixel(j, i, background);
                        }
                    }

                    for (int i = 0; i < GridSize; i++)
                    {
                        texture.SetPixel(0, i, main);
                        texture.SetPixel(i, 0, main);
                    }

                    texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
                    gridSprite = Sprite.Create(texture, new Rect(0, 0, GridSize, GridSize), Vector2.zero);
                }

                return gridSprite;
            }
        }

        public const int GridSize = 6;
        private static Sprite gridSprite = null;
    }
}
