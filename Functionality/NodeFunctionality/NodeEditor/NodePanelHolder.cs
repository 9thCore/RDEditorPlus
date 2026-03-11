using DG.Tweening;
using RDEditorPlus.Functionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityFileDialog;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor
{
    public abstract class NodePanelHolder : INodeWorkspace, ISerializableNodeWorkspace
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

        public INodeWorkspace.INode AddNode(string name, Vector2 position, string id = null)
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

        public bool TryGetNodeFromID(string id, out INodeWorkspace.INode result)
        {
            if (view.TryGetNodeFromID(id, out var node))
            {
                result = node;
                return true;
            }

            result = default;
            return false;
        }

        public void HandleLink(INodeWorkspace.INode input, string inputName, INodeWorkspace.INode output, string outputName)
        {
            var inputNode = (Node)input;
            var outputNode = (Node)output;

            var inputSide = inputNode.GetInput(inputName);
            var outputSide = outputNode.GetOutput(outputName);

            if (inputSide != null && outputSide != null)
            {
                inputSide.LinkIfNotYetLinked(outputSide);
            }
        }

        public ISerializableNodeWorkspace.INode[] GetDependencyOrderedNodes() => view.GetDependencyOrderedNodes();

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

            var clone = Object.Instantiate(template);
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

            grayButtonClone = Object.Instantiate(popup.steamPublishButton.gameObject);
            grayButtonClone.SetActive(false);
            var button2 = grayButtonClone.GetComponent<Button>();
            grayButtonCloneColorBlock = button2.colors;
            Object.DestroyImmediate(button2);
            Object.DestroyImmediate(grayButtonClone.GetComponentInChildren<RDStringToUIText>());

            Object.DestroyImmediate(popup.steamPublishButton.transform.parent.gameObject);
            Object.DestroyImmediate(popup.levelErrorPresentation.gameObject);
            Object.DestroyImmediate(popup.steamUpdateContainer.gameObject);
            Object.DestroyImmediate(popup.levelDataContainer.gameObject);
            Object.DestroyImmediate(popup.incompleteSettingsText.gameObject);
            Object.DestroyImmediate(popup);

            var sprite = clone.GetComponent<Image>().sprite;

            title = clone.GetComponentInChildren<Text>();
            Object.DestroyImmediate(title.GetComponent<RDStringToUIText>());
            Node.Font = title.font;
            NodeOutput.Sprite = NodeInput.Sprite = Node.Sprite = sprite;

            GameObject levelName = Object.Instantiate(title.gameObject, title.transform.parent);
            var rt3 = levelName.transform as RectTransform;
            rt3.anchoredPosition += Vector2.down * 10f;
            this.levelName = levelName.GetComponent<Text>();
            this.levelName.color = Color.gray;

            var button = clone.GetComponentInChildren<Button>();
            var buttonObject = button.gameObject;
            var colors = button.colors;
            Object.DestroyImmediate(button);

            button = buttonObject.AddComponent<Button>();
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                Toggle(show: false);
            });

            CloneButton(transform, "Save", () => ScheduleSave(forceAskForNewLocation: false),
                anchorMin: new Vector2(0.02f, 0.01f), anchorMax: new Vector2(0.24f, 0.08f));

            CloneButton(transform, "Save As", () => ScheduleSave(forceAskForNewLocation: true),
                anchorMin: new Vector2(0.26f, 0.01f), anchorMax: new Vector2(0.49f, 0.08f));

            CloneButton(transform, "New", NewButtonClick,
                anchorMin: new Vector2(0.02f, 0.10f), anchorMax: new Vector2(0.24f, 0.17f));

            CloneButton(transform, "Load", ScheduleLoad,
                anchorMin: new Vector2(0.26f, 0.10f), anchorMax: new Vector2(0.49f, 0.17f));

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

            GameObject titleClone = Object.Instantiate(title.gameObject, blocker.transform);
            blockerText = titleClone.GetComponent<Text>();
            blockerText.color = Color.white;
            blockerText.fontSize = 16;
            var rt = titleClone.transform as RectTransform;
            rt.anchorMin = rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.anchorMax = Vector2.one;

            var rt2 = blocker.transform as RectTransform;
            rt2.anchorMin = rt2.offsetMin = rt2.offsetMax = Vector2.zero;
            rt2.anchorMax = Vector2.one;
            rt2.localScale = Vector3.one;

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

        protected static GameObject CloneButton(Transform parent, string name, UnityAction call, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject clone = Object.Instantiate(grayButtonClone, parent);
            var button = clone.AddComponent<Button>();
            button.colors = grayButtonCloneColorBlock;
            button.onClick.AddListener(call);

            clone.GetComponentInChildren<Text>().text = name;

            var rt = clone.transform as RectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            clone.SetActive(true);
            return clone;
        }

        private static GameObject grayButtonClone;
        private static ColorBlock grayButtonCloneColorBlock;
    }

    public abstract class NodePanelHolder<XMLData> : NodePanelHolder where XMLData : NodeDataRoot, new()
    {
        public XMLData CreateXMLData()
        {
            XMLData data = new();
            data.Serialize(this);
            return data;
        }

        public void Simulate()
        {
            CreateXMLData().Deserialize(new NodeSimulator());
        }

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
