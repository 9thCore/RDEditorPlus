using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid
{
    public class NodeGridView : MonoBehaviour
    {
        public static void ShowDescription(string text) => Description.Show(text);
        public static void HideDescription() => Description.Hide();
        public static void MoveDescription(Vector2 position) => Description.Move(position);

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
            mask.AddComponent<NodeGridViewEventTrigger>().Setup(component.grid);

            view.SetActive(true);
            return component;
        }

        public Node AddNode(GameObject prefab, Vector2 position, string id) => grid.AddNode(prefab, position, id);

        public void Reset()
        {
            Description.Hide();
            grid.Reset();
        }

        public async Task SaveAsync(XmlWriter writer)
        {
            await grid.SaveAsync(writer);
        }

        public void Undo() => grid.Undo();
        public void Redo() => grid.Redo();
        public void ClearUndo() => grid.ClearUndo();

        public void Clear() => grid.Clear();

        public bool TryGetNodeFromID(string id, out Node result) => grid.TryGetNodeFromID(id, out result);

        public Node[] GetDependencyOrderedNodes() => grid.GetDependencyOrderedNodes();

        private NodeGrid grid;

        private static DescriptionHolder Description
        {
            get
            {
                if (description == null || !description.Valid())
                {
                    description = DescriptionHolder.Create();
                }

                return description;
            }
        }
        private static DescriptionHolder description;

        private record DescriptionHolder(GameObject GameObject, RectTransform Transform, Text Text)
        {
            public bool Valid() => GameObject != null;

            public void Show(string text)
            {
                if (text.IsNullOrEmpty())
                {
                    Hide();
                    return;
                }

                Text.text = text;
                GameObject.SetActive(true);
            }

            public void Hide()
            {
                GameObject.SetActive(false);
            }

            public void Move(Vector2 position)
            {
                Transform.position = position;
            }

            public static DescriptionHolder Create()
            {
                GameObject description = new("description");
                description.SetActive(false);

                var descriptionImage = description.AddComponent<Image>();
                descriptionImage.sprite = AssetUtil.ButtonSprite;
                descriptionImage.type = Image.Type.Sliced;
                descriptionImage.color = "363636FF".HexToColor();
                descriptionImage.raycastTarget = false;

                description.AddComponent<EightSidedOutline>().effectColor = Color.black;
                description.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);
                description.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var descriptionGroup = description.AddComponent<HorizontalLayoutGroup>();
                descriptionGroup.childForceExpandWidth = true;
                descriptionGroup.childForceExpandHeight = false;
                descriptionGroup.childControlWidth = true;
                descriptionGroup.childControlHeight = true;
                descriptionGroup.padding = new RectOffset(4, 4, 4, 4);

                var descriptionRT = description.transform as RectTransform;
                descriptionRT.SetParent(scnEditor.instance.canvasRectTransform);
                descriptionRT.localScale = Vector3.one;
                descriptionRT.pivot = new Vector2(0f, 1f);

                GameObject descriptionTextObject = new("text");

                var descriptionText = descriptionTextObject.AddComponent<Text>();
                descriptionText.ApplyRDFont();
                descriptionText.alignment = TextAnchor.UpperCenter;

                descriptionTextObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var descriptionTextRT = descriptionTextObject.transform as RectTransform;
                descriptionTextRT.SetParent(descriptionRT);
                descriptionTextRT.localScale = Vector3.one;
                descriptionTextRT.offsetMin = descriptionTextRT.offsetMax = Vector2.zero;
                descriptionTextRT.pivot = new Vector2(0.5f, 1f);

                return new(description, descriptionRT, descriptionText);
            }
        }
    }
}
