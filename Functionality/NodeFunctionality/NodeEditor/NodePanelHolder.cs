using RDEditorPlus.Functionality.ArbitraryPanel;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityFileDialog;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor
{
    public abstract class NodePanelHolder : ArbitraryPanelHolder, INodeWorkspace, ISerializableNodeWorkspace
    {
        public static NodePanelHolder CurrentPanel = null;

        public static bool CurrentlyLoading => CurrentPanel != null && CurrentPanel.state == State.Loading;
        public static string LastUsedDirectory => CurrentPanel == null ? string.Empty : CurrentPanel.GetLastUsedDirectory();

        public abstract string DefaultFilename { get; }
        public abstract string FileDescription { get; }
        public abstract string SaveFileText { get; }
        public abstract string LoadFileText { get; }
        public abstract string[] Extensions { get; }

        protected abstract Task SaveAsync();
        protected abstract string SavePostfix { get; }

        public INodeWorkspace.INode AddNode(string name, Vector2 position, string id = null)
        {
            var prefab = NodeLibrary.Instance.GetPrefab(name);
            if (prefab == null)
            {
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
                    GetLastUsedDirectory(),
                    DefaultFilename,
                    FileDescription,
                    Extensions,
                    SaveFileText);

                if (location.IsNullOrEmpty())
                {
                    return;
                }

                SetLastUsedDirectory(Path.GetDirectoryName(location));
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

            string location = FileBrowser.PickFile(
                    GetLastUsedDirectory(),
                    FileDescription,
                    Extensions,
                    LoadFileText);

            if (location.IsNullOrEmpty())
            {
                return;
            }

            SetLastUsedDirectory(Path.GetDirectoryName(location));
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

            view.ClearUndo();

            SetLevelName();
            SetState(State.Idle);
        }

        public void Clear()
        {
            view.Clear();
            view.ClearUndo();
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

        public override void OnShow()
        {
            view.Reset();
            CurrentPanel = this;
        }

        public override void OnHide()
        {
            CurrentPanel = null;
        }

        public void OnUpdate()
        {
            if (!scnEditor.instance.userIsEditingAnInputField)
            {
                InputUpdate();
            }
        }

        protected abstract void HandleDeserialization(Stream stream);

        protected virtual void InputUpdate()
        {
            HaveAnyDefaultInputsPassed();
        }

        protected bool HaveAnyDefaultInputsPassed()
        {
            if (RDEditorUtils.CheckForKeyCombo(control: true, shift: false, KeyCode.N))
            {
                NewButtonClick();
                return true;
            }
            else if (RDEditorUtils.CheckForKeyCombo(control: true, shift: false, KeyCode.O))
            {
                ScheduleLoad();
                return true;
            }
            else if (RDEditorUtils.CheckForKeyCombo(control: true, shift: true, KeyCode.S))
            {
                ScheduleSave(forceAskForNewLocation: true);
                return true;
            }
            else if (RDEditorUtils.CheckForKeyCombo(control: true, shift: false, KeyCode.S))
            {
                ScheduleSave(forceAskForNewLocation: false);
                return true;
            }
            else if (RDEditorUtils.CheckForKeyCombo(control: true, shift: false, KeyCode.Z))
            {
                view.Undo();
                return true;
            }
            else if (RDEditorUtils.CheckForKeyCombo(control: true, shift: true, KeyCode.Z)
                || RDEditorUtils.CheckForKeyCombo(control: true, shift: false, KeyCode.Y))
            {
                view.Redo();
                return true;
            }

            return false;
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

        private string GetLastUsedDirectory() => SaveUtil.GetString(LastDirectorySaveKey, string.Empty);
        private void SetLastUsedDirectory(string path) => SaveUtil.SetString(LastDirectorySaveKey, path);
        private string LastDirectorySaveKey => $"PanelHolder{SavePostfix}_LastDirectory";

        protected NodePanelHolder() : base()
        {
            GameObject levelName = Object.Instantiate(title.gameObject, title.transform.parent);
            var rt3 = levelName.transform as RectTransform;
            rt3.anchoredPosition += Vector2.down * 10f;
            this.levelName = levelName.GetComponent<Text>();
            this.levelName.color = Color.gray;

            CloneButton(rectTransform, "Save", () => ScheduleSave(forceAskForNewLocation: false),
                anchorMin: new Vector2(0.02f, 0.01f), anchorMax: new Vector2(0.24f, 0.08f));

            CloneButton(rectTransform, "Save As", () => ScheduleSave(forceAskForNewLocation: true),
                anchorMin: new Vector2(0.26f, 0.01f), anchorMax: new Vector2(0.49f, 0.08f));

            CloneButton(rectTransform, "New", NewButtonClick,
                anchorMin: new Vector2(0.02f, 0.10f), anchorMax: new Vector2(0.24f, 0.17f));

            CloneButton(rectTransform, "Load", ScheduleLoad,
                anchorMin: new Vector2(0.26f, 0.10f), anchorMax: new Vector2(0.49f, 0.17f));

            var sprite = gameObject.GetComponent<Image>().sprite;

            view = NodeGridView.Create(rectTransform, sprite, this);

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
        }

        protected readonly NodeGridView view;
        protected readonly Text levelName;
        protected readonly GameObject blocker;
        protected readonly Text blockerText;

        protected string savedLevelName = string.Empty;
        protected State state = State.Idle;

        protected enum State
        {
            Idle,
            Saving,
            Loading
        }
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
