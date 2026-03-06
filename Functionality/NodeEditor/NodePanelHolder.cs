using DG.Tweening;
using RDEditorPlus.Functionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityFileDialog;

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

        public void ScheduleSave()
        {
            if (state != State.Idle)
            {
                return;
            }

            scnEditor.PauseIfUnpaused();

            string location = FileBrowser.SaveFile(
                scnEditor.GetLastUsedFolder(),
                DefaultFilename,
                "Rhythm Doctor level merge data",
                Extensions,
                "Save level merge data");

            if (location == string.Empty)
            {
                return;
            }

            state = State.Saving;
            Task.Run(() => SaveAsync(location));
        }

        public async Task SaveAsync(string location)
        {
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                Async = true
            };

            using var writer = XmlWriter.Create(location, settings);

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(MergeDataKey);

            await view.SaveAsync(writer);

            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();

            writer.Close();

            state = State.Saving;
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

            GameObject buttonClone = GameObject.Instantiate(popup.steamPublishButton.gameObject);
            var button2 = buttonClone.GetComponent<Button>();
            var colors2 = button2.colors;
            GameObject.DestroyImmediate(button2);

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

            GameObject save = GameObject.Instantiate(buttonClone, transform);
            var button3 = save.AddComponent<Button>();
            button3.colors = colors2;
            button3.onClick.AddListener(ScheduleSave);

            var text = save.GetComponentInChildren<Text>();
            text.text = "Save merge data...";
            GameObject.DestroyImmediate(text.GetComponent<RDStringToUIText>());

            var rt = save.transform as RectTransform;
            rt.anchorMin = new Vector2(0.02f, 0.01f);
            rt.anchorMax = new Vector2(0.48f, 0.08f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            GameObject load = GameObject.Instantiate(buttonClone, transform);
            var button4 = load.AddComponent<Button>();
            button4.colors = colors2;
            //button4.onClick.AddListener(ScheduleImport);

            var text2 = load.GetComponentInChildren<Text>();
            text2.text = "Load merge data...";
            GameObject.DestroyImmediate(text2.GetComponent<RDStringToUIText>());

            var rt2 = load.transform as RectTransform;
            rt2.anchorMin = new Vector2(0.02f, 0.10f);
            rt2.anchorMax = new Vector2(0.48f, 0.17f);
            rt2.offsetMin = rt2.offsetMax = Vector2.zero;

            view = NodeGridView.Create(transform, sprite, this);
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

        private State state = State.Idle;

        private enum State
        {
            Idle,
            Saving
        }

        public const string MergeDataKey = "MergeData";

        private const string DefaultFilename = "level";
        private static readonly string[] Extensions = ["rdmerge"];
    }
}
