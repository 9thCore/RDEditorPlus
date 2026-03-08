using DG.Tweening;
using RDEditorPlus.Functionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityFileDialog;

namespace RDEditorPlus.Functionality.NodeEditor
{
    public abstract class NodePanelHolder
    {
        public abstract string DefaultFilename { get; }
        public abstract string[] Extensions { get; }

        protected abstract Task SaveAsync();

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

        public Node AddNode(string name, Vector2 position, string id = null)
        {
            if (!nodePrefabs.TryGetValue(name, out var prefab))
            {
                Plugin.LogError($"Tried to add node '{name}', but it is not registered for {GetType().FullName}!");
                return null;
            }

            return view.AddNode(prefab, position, id);
        }

        public void ScheduleSave(bool forceAskForNewLocation = false)
        {
            if (state != State.Idle)
            {
                return;
            }

            scnEditor.PauseIfUnpaused();

            if (forceAskForNewLocation
                || savedLevelName.IsNullOrEmpty()
                || !Directory.Exists(Path.GetDirectoryName(savedLevelName)))
            {
                string location = FileBrowser.SaveFile(
                    scnEditor.GetLastUsedFolder(),
                    DefaultFilename,
                    "Rhythm Doctor level merge data",
                    Extensions,
                    "Save level merge data");

                if (location.IsNullOrEmpty())
                {
                    return;
                }

                savedLevelName = location;
            }

            SetLevelName();

            SetState(State.Saving);
            Task.Run(SaveAsync);
        }

        public void ScheduleLoad()
        {
            if (state != State.Idle)
            {
                return;
            }

            scnEditor.PauseIfUnpaused();

            string location = FileBrowser.PickFile(
                    scnEditor.GetLastUsedFolder(),
                    "Rhythm Doctor level merge data",
                    Extensions,
                    "Load level merge data");

            if (location.IsNullOrEmpty())
            {
                return;
            }

            SetState(State.Loading);
            Clear();

            using var stream = File.OpenRead(location);

            try
            {
                HandleDeserialization(stream);
                savedLevelName = location;
            }
            catch (System.Exception e)
            {
                Plugin.LogError($"Failed loading {Path.GetFileName(location)}:\n{e}");

                Clear();
                savedLevelName = null;
            }

            SetLevelName();
            SetState(State.Idle);
        }

        public void Clear()
        {
            view.Clear();
        }

        public void NewButtonClick()
        {
            if (state != State.Idle)
            {
                return;
            }

            Clear();

            savedLevelName = string.Empty;
            SetLevelName();
        }

        public bool TryGetNodeFromID(string id, out Node result) => view.TryGetNodeFromID(id, out result);
        protected abstract void HandleDeserialization(Stream stream);
        

        protected void AllowNodes(params string[] names)
        {
            foreach (var name in names)
            {
                nodePrefabs.Add(name, NodeLibrary.Instance.GetPrefab(name));
            }
        }

        protected void SetLevelName()
        {
            if (savedLevelName.IsNullOrEmpty())
            {
                levelName.text = "unsaved";
                return;
            }

            levelName.text = Path.GetFileName(savedLevelName);
        }

        protected void SetState(State state)
        {
            this.state = state;

            if (state == State.Idle)
            {
                blocker.SetActive(false);
                return;
            }

            blocker.SetActive(true);
            blockerText.text = state.ToString();
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

            GameObject levelName = GameObject.Instantiate(title.gameObject, title.transform.parent);
            var rt3 = levelName.transform as RectTransform;
            rt3.anchoredPosition += Vector2.down * 10f;
            this.levelName = levelName.GetComponent<Text>();
            this.levelName.color = Color.gray;

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
            button3.onClick.AddListener(() => ScheduleSave(forceAskForNewLocation: false));

            var text = save.GetComponentInChildren<Text>();
            text.text = "Save";
            GameObject.DestroyImmediate(text.GetComponent<RDStringToUIText>());

            var rt = save.transform as RectTransform;
            rt.anchorMin = new Vector2(0.02f, 0.01f);
            rt.anchorMax = new Vector2(0.24f, 0.08f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            GameObject saveAs = GameObject.Instantiate(buttonClone, transform);
            var button5 = saveAs.AddComponent<Button>();
            button5.colors = colors2;
            button5.onClick.AddListener(() => ScheduleSave(forceAskForNewLocation: true));

            var text3 = saveAs.GetComponentInChildren<Text>();
            text3.text = "Save As";
            GameObject.DestroyImmediate(text3.GetComponent<RDStringToUIText>());

            var rt4 = saveAs.transform as RectTransform;
            rt4.anchorMin = new Vector2(0.26f, 0.01f);
            rt4.anchorMax = new Vector2(0.49f, 0.08f);
            rt4.offsetMin = rt4.offsetMax = Vector2.zero;

            GameObject newObj = GameObject.Instantiate(buttonClone, transform);
            var button6 = newObj.AddComponent<Button>();
            button6.colors = colors2;
            button6.onClick.AddListener(NewButtonClick);

            var text4 = newObj.GetComponentInChildren<Text>();
            text4.text = "New";
            GameObject.DestroyImmediate(text4.GetComponent<RDStringToUIText>());

            var rt5 = newObj.transform as RectTransform;
            rt5.anchorMin = new Vector2(0.02f, 0.10f);
            rt5.anchorMax = new Vector2(0.24f, 0.17f);
            rt5.offsetMin = rt5.offsetMax = Vector2.zero;

            GameObject load = GameObject.Instantiate(buttonClone, transform);
            var button4 = load.AddComponent<Button>();
            button4.colors = colors2;
            button4.onClick.AddListener(ScheduleLoad);

            var text2 = load.GetComponentInChildren<Text>();
            text2.text = "Load";
            GameObject.DestroyImmediate(text2.GetComponent<RDStringToUIText>());

            var rt2 = load.transform as RectTransform;
            rt2.anchorMin = new Vector2(0.26f, 0.10f);
            rt2.anchorMax = new Vector2(0.49f, 0.17f);
            rt2.offsetMin = rt2.offsetMax = Vector2.zero;

            view = NodeGridView.Create(transform, sprite, this);
            gameObject = clone;
            rectTransform = clone.transform as RectTransform;

            blocker = new("blocker");
            blocker.SetActive(false);
            blocker.transform.SetParent(view.transform);
            var image = blocker.AddComponent<Image>();
            image.type = Image.Type.Tiled;
            image.sprite = sprite;
            image.color = Color.black.WithAlpha(0.5f);

            GameObject titleClone = GameObject.Instantiate(title.gameObject, blocker.transform);
            blockerText = titleClone.GetComponent<Text>();
            blockerText.color = Color.white;
            blockerText.fontSize = 16;
            var rt6 = titleClone.transform as RectTransform;
            rt6.anchorMin = rt6.offsetMin = rt6.offsetMax = Vector2.zero;
            rt6.anchorMax = Vector2.one;

            var rt7 = blocker.transform as RectTransform;
            rt7.anchorMin = rt7.offsetMin = rt7.offsetMax = Vector2.zero;
            rt7.anchorMax = Vector2.one;
            rt7.localScale = Vector3.one;

            SetLevelName();

            AllowNodes("Test");
        }

        protected readonly GameObject gameObject;
        protected readonly RectTransform rectTransform;
        protected readonly NodeGridView view;
        protected readonly Text title;
        protected readonly Text levelName;
        protected readonly GameObject blocker;
        protected readonly Text blockerText;
        protected readonly Dictionary<string, GameObject> nodePrefabs = new();

        protected string savedLevelName = string.Empty;
        protected State state = State.Idle;

        protected enum State
        {
            Idle,
            Saving,
            Loading
        }
    }

    public abstract class NodePanelHolder<XMLData> : NodePanelHolder where XMLData : NodeDataRoot
    {
        protected override async Task SaveAsync()
        {
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                Async = true
            };

            using var writer = XmlWriter.Create(savedLevelName, settings);

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(typeof(XMLData).Name);

            await view.SaveAsync(writer);

            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();

            writer.Close();

            SetState(State.Idle);
        }

        protected override void HandleDeserialization(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(XMLData));

            if (serializer.Deserialize(stream) is not XMLData result)
            {
                throw new InvalidDataException($"Could not deserialise as {typeof(XMLData).FullName}");
            }

            result.Deserialize(this);
        }
    }
}
