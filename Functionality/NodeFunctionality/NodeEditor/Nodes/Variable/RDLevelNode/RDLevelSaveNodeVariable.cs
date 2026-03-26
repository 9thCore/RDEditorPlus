using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using UnityEngine;
using UnityEngine.UI;
using UnityFileDialog;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable.RDLevelNode
{
    public class RDLevelSaveNodeVariable : NodeVariable<RDLevelSaveNodeVariable, RDLevelSaveFile>
    {
        public override bool CanSave() => !CurrentValue.Equals(initialValue);

        public void SelectLevel()
        {
            string location = FileBrowser.SaveFile(
                    NodePanelHolder.LastUsedDirectory,
                    "output.rdlevel",
                    RDString.Get("editor.dialog.levelFileFormat"),
                    ["rdlevel"],
                    RDString.Get("editor.dialog.openFile"));

            if (location == null)
            {
                return;
            }

            OnVariableChange(location);
        }

        protected override void OnVariableChange(string path)
        {
            CurrentValue = new RDLevelSaveFile(path);
            SetRepresentation(CurrentValue.LevelName);
        }

        protected override void SetInitialValue(object initialValue)
        {
            ((Text)inputField.placeholder).text = "unset";
            base.SetInitialValue(default(RDLevelSaveFile));
        }

        protected override void SetRepresentation(string value)
        {
            inputField.SetTextWithoutNotify(value);
        }

        private void Awake()
        {
            browseButton.onClick.AddListener(SelectLevel);
        }

        [SerializeField]
        private Button browseButton;

        public static GameObject VariablePrefab
        {
            get
            {
                if (variablePrefab == null)
                {
                    variablePrefab = Instantiate(InputFieldTextPlaceholderVariable);
                    variablePrefab.name += "RDLevel";

                    var variable = variablePrefab.GetComponent<RDLevelSaveNodeVariable>();
                    variable.type = Node.Type.RDLevelSaveFile;

                    var inputField = variable.inputField;
                    inputField.readOnly = true;
                    inputField.characterValidation = InputField.CharacterValidation.None;
                    inputField.contentType = InputField.ContentType.Standard;
                    inputField.lineType = InputField.LineType.SingleLine;
                    inputField.customCaretColor = true;
                    inputField.caretColor = Color.white.WithAlpha(0f);

                    inputField.GetComponent<Image>().color = "2F2F2FFF".HexToColor();
                    inputField.textComponent.color = Color.white;

                    ((Text)inputField.placeholder).color = Color.white.WithAlpha(0.33f);

                    var button = AddBrowseButton(inputField.transform as RectTransform)
                        .AddComponent<Button>();

                    button.colors = button.colors with
                    {
                        pressedColor = Color.Lerp(Color.yellow, Color.red, 0.5f),
                        highlightedColor = Color.yellow
                    };

                    variable.browseButton = button;
                }

                return variablePrefab;
            }
        }

        private static GameObject variablePrefab;
    }
}
