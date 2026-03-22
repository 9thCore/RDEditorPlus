using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Conditionals;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Constant;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Events;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.FileIO;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Maths;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Rows;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.Sprites;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Grid
{
    public class NodeDropdown : MonoBehaviour
    {
        private static NodeDropdown instance;
        public static NodeDropdown Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject holder = CreateDropdown("Root");
                    instance = holder.AddComponent<NodeDropdown>();

                    Transform transform = holder.transform;

                    #region file
                    GameObject file = CreateDropdown("File");
                    AddButton(file.transform, "Load RD Level", instance.CreateNode<Node_LoadRDLevel>);
                    AddButton(file.transform, "Save RD Level", instance.CreateNode<Node_SaveRDLevel>);

                    AddCategoryButton(transform, "File I/O", file);
                    #endregion

                    #region constants
                    GameObject constants = CreateDropdown("Constant");

                    #region constants.math
                    GameObject constants_math = CreateDropdown("Math");

                    AddButton(constants_math.transform, "Int", instance.CreateNode<Node_IntConstant>);
                    AddButton(constants_math.transform, "Float", instance.CreateNode<Node_FloatConstant>);
                    AddButton(constants_math.transform, "Expression", instance.CreateNode<Node_FloatExpressionConstant>);
                    AddButton(constants_math.transform, "Float2", instance.CreateNode<Node_Float2Constant>);
                    AddButton(constants_math.transform, "Expression2", instance.CreateNode<Node_FloatExpression2Constant>);

                    AddCategoryButton(constants.transform, "Math", constants_math);
                    #endregion

                    //AddCategoryButton(transform, "Constant", constants);
                    #endregion

                    #region sprites
                    GameObject sprites = CreateDropdown("Sprite");
                    AddButton(sprites.transform, "Merge", instance.CreateNode<Node_MergeSprites>);

                    AddCategoryButton(transform, "Sprites", sprites);
                    #endregion

                    #region rows
                    GameObject rows = CreateDropdown("Row");
                    AddButton(rows.transform, "Merge", instance.CreateNode<Node_MergeRows>);

                    AddCategoryButton(transform, "Rows", rows);
                    #endregion

                    #region events
                    GameObject events = CreateDropdown("Event");
                    AddButton(events.transform, "Bar Filter", instance.CreateNode<Node_BarFilter>);
                    AddButton(events.transform, "Tab Filter", instance.CreateNode<Node_TabFilter>);
                    AddButton(events.transform, "Merge", instance.CreateNode<Node_MergeEvents>);

                    AddCategoryButton(transform, "Events", events);
                    #endregion

                    #region conditionals
                    GameObject conditionals = CreateDropdown("Conditional");
                    AddButton(conditionals.transform, "Merge", instance.CreateNode<Node_MergeConditionals>);

                    AddCategoryButton(transform, "Conditions", conditionals);
                    #endregion

                    #region math
                    GameObject math = CreateDropdown("Math");
                    AddButton(math.transform, "Binary", instance.CreateNode<Node_MathBinary>);

                    //AddCategoryButton(transform, "Math", math);
                    #endregion

                    GameObject blocker = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(NodeDropdown)}_Blocker");
                    blocker.SetActive(false);
                    blocker.transform.SetParent(scnEditor.instance.canvasRectTransform);

                    var blockerImage = blocker.AddComponent<Image>();
                    blockerImage.color = Color.white.WithAlpha(0f);

                    blocker.AddComponent<Button>().onClick.AddListener(instance.BlockerClick);
                    blocker.transform.localScale *= 2000f;
                    instance.blocker = blocker.transform;
                }

                return instance;
            }
        }

        public void Activate(Vector2 position, NodeGrid grid)
        {
            transform.position = position;
            this.grid = grid;
            gameObject.SetActive(true);
            blocker.gameObject.SetActive(true);

            instance.blocker.SetAsLastSibling();
            transform.SetAsLastSibling();
        }

        public GameObject CurrentSubtree { get; set; }

        private void CreateNode<T>()
            where T : Node_Base
        {
            string name = typeof(T).Name.Substring("Node_".Length);
            grid.AddNodeAtPointerPosition(name, transform.position);

            CurrentSubtree.SetActive(false);
            blocker.gameObject.SetActive(false);
        }

        private void BlockerClick()
        {
            if (CurrentSubtree != null)
            {
                CurrentSubtree.SetActive(false);
            }

            gameObject.SetActive(false);
            blocker.gameObject.SetActive(false);
        }

        private NodeGrid grid;
        private Transform blocker;

        private static void AddButton(Transform dropdown, string name, UnityAction action)
        {
            GameObject button = GameObject.Instantiate(Button, dropdown);

            var text = button.GetComponent<Text>();
            text.text = name;

            button.AddComponent<Button>().onClick.AddListener(action);
        }

        private static void AddCategoryButton(Transform dropdown, string name, GameObject target)
        {
            GameObject categoryButton = GameObject.Instantiate(CategoryButton, dropdown);

            var child = categoryButton.transform.Find("name");
            var text = child.GetComponent<Text>();
            text.text = name;

            categoryButton.AddComponent<NodeDropdownCategoryEventTrigger>()
                .Setup(instance, dropdown.gameObject, target);
        }

        private static GameObject CreateDropdown(string name)
        {
            GameObject dropdown = GameObject.Instantiate(Dropdown, scnEditor.instance.canvasRectTransform);
            dropdown.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(NodeDropdown)}_{name}";
            return dropdown;
        }

        private static GameObject Dropdown
        {
            get
            {
                if (dropdown == null)
                {
                    dropdown = new();
                    dropdown.SetActive(false);

                    dropdown.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    dropdown.AddComponent<EightSidedOutline>().effectColor = Color.black;
                    dropdown.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);

                    var image = dropdown.AddComponent<Image>();
                    image.sprite = AssetUtil.ButtonSprite;
                    image.type = Image.Type.Sliced;
                    image.color = "7F7F7FFF".HexToColor();

                    var group = dropdown.AddComponent<VerticalLayoutGroup>();
                    group.childForceExpandWidth = true;
                    group.childForceExpandHeight = false;
                    group.childControlWidth = true;
                    group.childControlHeight = true;
                    group.padding = new RectOffset(2, 2, 2, 2);
                    group.spacing = 4f;

                    var transform = dropdown.transform as RectTransform;
                    transform.pivot = new Vector2(0.5f, 1f);
                    transform.sizeDelta = new Vector2(50f, 0f);
                }

                return dropdown;
            }
        }

        private static GameObject Button
        {
            get
            {
                if (button == null)
                {
                    GameObject go = new();

                    go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var text = go.AddComponent<Text>();
                    text.font = AssetUtil.StandardFont;
                    text.fontSize = 8;
                    text.alignment = TextAnchor.MiddleCenter;

                    var transform = go.transform as RectTransform;
                    transform.anchorMin = Vector2.zero;
                    transform.anchorMax = Vector2.one;

                    go.AddComponent<EightSidedOutline>().effectColor = Color.black;

                    button = go;
                }

                return button;
            }
        }

        private static GameObject CategoryButton
        {
            get
            {
                if (categoryButton == null)
                {
                    GameObject category = new();

                    category.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var group = category.AddComponent<HorizontalLayoutGroup>();
                    group.childForceExpandWidth = true;
                    group.childForceExpandHeight = false;
                    group.childControlWidth = true;
                    group.childControlHeight = true;
                    group.reverseArrangement = true;

                    GameObject arrow = new(">");

                    var arrowText = arrow.AddComponent<Text>();
                    arrowText.text = ">";
                    arrowText.font = AssetUtil.StandardFont;
                    arrowText.fontSize = 8;
                    arrowText.alignment = TextAnchor.MiddleRight;

                    var arrowRT = arrow.transform as RectTransform;
                    arrowRT.SetParent(category.transform);
                    arrowRT.pivot = new Vector2(1f, 0.5f);
                    arrowRT.anchorMin = new Vector2(1f, 0f);
                    arrowRT.anchorMax = Vector2.one;

                    GameObject name = new("name");

                    var nameText = name.AddComponent<Text>();
                    nameText.font = AssetUtil.StandardFont;
                    nameText.fontSize = 8;
                    nameText.alignment = TextAnchor.MiddleCenter;

                    var nameRT = name.transform as RectTransform;
                    nameRT.SetParent(category.transform);
                    nameRT.pivot = new Vector2(0f, 0.5f);
                    nameRT.anchorMin = Vector2.zero;
                    nameRT.anchorMax = new Vector2(0f, 1f);
                    nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;

                    name.AddComponent<EightSidedOutline>().effectColor = Color.black;
                    arrow.AddComponent<EightSidedOutline>().effectColor = Color.black;

                    categoryButton = category;
                }

                return categoryButton;
            }
        }

        private static GameObject dropdown;
        private static GameObject button;
        private static GameObject categoryButton;
    }
}
