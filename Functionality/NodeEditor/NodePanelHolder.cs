using DG.Tweening;
using RDEditorPlus.Functionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using RDLevelEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeEditor
{
    public class NodePanelHolder
    {
        public void Toggle(bool show)
        {
            if (show)
            {
                gameObject.SetActive(true);
                view.Reset();
            }

            Vector3 start = show ? Vector3.zero : Vector3.one;
            Vector3 end = show ? Vector3.one : Vector3.zero;

            float time = show ? RDConstants.data.roomsSelectionPopup_showAnimDuration : RDConstants.data.roomsSelectionPopup_hideAnimDuration;
            Ease ease = show ? RDConstants.data.roomsSelectionPopup_showAnimEase : RDConstants.data.roomsSelectionPopup_hideAnimEase;

            rectTransform.DOKill(true);
            rectTransform.localScale = start;
            rectTransform.DOScale(end, time).SetEase(ease).SetUpdate(UpdateType.Normal, true)
                .OnComplete(delegate
                {
                    if (!show)
                    {
                        gameObject.SetActive(false);
                    }
                });
        }

        public void AddNode(string name, Vector2 position)
        {
            if (!nodePrefabs.TryGetValue(name, out var prefab))
            {
                Plugin.LogError($"Tried to add node '{name}', but it is not registered for {GetType().FullName}!");
                return;
            }

            view.AddNode(prefab, position);
        }

        protected void PrepareNodePrefab(string name, IEnumerable<NodeInput.Data> inputs, IEnumerable<NodeOutput.Data> outputs)
        {
            nodePrefabs.Add(name, Node.PreparePrefab(name, inputs, outputs));
        }

        protected NodePanelHolder()
        {
            var template = scnEditor.instance.publishPopup.gameObject;
            var templateTransform = template.transform;

            var clone = GameObject.Instantiate(template);
            clone.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_{GetType().Name}";

            var transform = clone.transform;

            transform.SetParent(templateTransform.parent);
            transform.localPosition = templateTransform.localPosition;
            transform.localRotation = templateTransform.localRotation;
            transform.localScale = templateTransform.localScale;

            var popup = clone.GetComponent<RDPublishPopup>();
            popup.enabled = false;

            popup.onDialogScreenBlocker.raycastTarget = true;
            popup.onDialogScreenBlocker.transform.SetAsFirstSibling();

            GameObject.DestroyImmediate(popup.steamPublishButton.transform.parent.gameObject);
            GameObject.DestroyImmediate(popup.levelErrorPresentation.gameObject);
            GameObject.DestroyImmediate(popup.steamUpdateContainer.gameObject);
            GameObject.DestroyImmediate(popup.levelDataContainer.gameObject);
            GameObject.DestroyImmediate(popup.incompleteSettingsText.gameObject);
            GameObject.DestroyImmediate(popup);

            var sprite = clone.GetComponent<Image>().sprite;

            title = clone.GetComponentInChildren<Text>();
            GameObject.DestroyImmediate(title.GetComponent<RDStringToUIText>());
            Node.Font = title.font;
            NodeOutput.Sprite = NodeInput.Sprite = Node.Sprite = sprite;

            var button = clone.GetComponentInChildren<Button>();
            var buttonObject = button.gameObject;
            var colors = button.colors;
            GameObject.DestroyImmediate(button);

            button = buttonObject.AddComponent<Button>();
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                Toggle(show: false);
            });

            view = NodeGridView.Create(transform, sprite);
            gameObject = clone;
            rectTransform = clone.transform as RectTransform;

            PrepareNodePrefab("Test",
                [
                    new(Node.Type.Float, "in")
                ],
                [
                    new(Node.Type.Float, "out")
                ]);
        }

        protected readonly GameObject gameObject;
        protected readonly RectTransform rectTransform;
        protected readonly NodeGridView view;
        protected readonly Text title;
        protected readonly Dictionary<string, GameObject> nodePrefabs = new();
    }
}
