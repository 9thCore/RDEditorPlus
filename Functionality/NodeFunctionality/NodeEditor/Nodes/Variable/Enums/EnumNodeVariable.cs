using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Variable.Enums
{
    public abstract class EnumNodeVariable<ScriptType, TEnum> : NodeVariable<ScriptType, TEnum>
        where ScriptType : EnumNodeVariable<ScriptType, TEnum>
        where TEnum : struct, Enum
    {
        protected override void OnVariableChange(string text)
        {
            if (Enum.TryParse(text, out TEnum value))
            {
                currentValue = value;
            }
            else
            {
                currentValue = initialValue;
            }

            base.OnVariableChange(text);
        }

        protected override void SetInitialValue(object initialValue)
        {
            dropdown.value = Convert.ToInt32(initialValue);
        }

        protected override void SetRepresentation(string value)
        {
            dropdown.value = Convert.ToInt32(Enum.Parse(typeof(TEnum), value));
        }

        private void OnVariableChange(int index)
        {
            currentValue = Values[index];
            SetRepresentation(currentValue.ToString());
        }

        private void Awake()
        {
            dropdown.onValueChanged.AddListener(OnVariableChange);
        }

        [SerializeField]
        protected Dropdown dropdown;

        protected static GameObject GetCachedPrefab(Node.Type type)
        {
            if (variablePrefab == null)
            {
                variablePrefab = Instantiate(EnumNodeVariablePrefabHolder.GetCachedPrefab(BaseVariable));

                var variable = variablePrefab.GetComponent<ScriptType>();
                variable.type = type;

                variable.dropdown = variablePrefab.GetComponentInChildren<Dropdown>();
                variable.dropdown.AddOptionsWithEnumValues(typeof(TEnum), localize: false);
            }

            return variablePrefab;
        }

        private static GameObject variablePrefab;

        private static readonly TEnum[] Values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
    }

    // ugly hack lol
    internal static class EnumNodeVariablePrefabHolder
    {
        public static GameObject GetCachedPrefab(GameObject baseVariable)
        {
            if (cachedPrefab == null)
            {
                cachedPrefab = GameObject.Instantiate(GetPrefab(baseVariable));
            }

            return cachedPrefab;
        }

        private static GameObject GetPrefab(GameObject baseVariable)
        {
            GameObject variablePrefab = GameObject.Instantiate(baseVariable);
            variablePrefab.name += "Enum";

            GameObject controlInstance = GameObject.Instantiate(RDConstants.data.dropdownControlPrefab);

            var dropdown = controlInstance.GetComponentInChildren<Dropdown>();

            var dropdownRT = dropdown.transform as RectTransform;
            dropdownRT.SetParent(variablePrefab.transform);
            dropdownRT.localPosition = Vector3.zero;
            dropdownRT.localScale = Vector3.one;
            dropdownRT.anchorMin = new Vector2(0.5f, 0f);
            dropdownRT.anchorMax = new Vector2(1f, 1f);
            dropdownRT.offsetMin = new Vector2(2f, 0f);
            dropdownRT.offsetMax = new Vector2(-2f, 0f);

            GameObject.DestroyImmediate(dropdown.GetComponent<EditorDropdown>());
            GameObject.DestroyImmediate(controlInstance);

            return variablePrefab;
        }

        private static GameObject cachedPrefab;
    }
}
