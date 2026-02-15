using RDLevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDEditorPlus.ExtraData
{
    internal class PropertyStorage : MonoBehaviour
    {
        private static PropertyStorage instance;
        public static PropertyStorage Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject storage = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(PropertyStorage)}");
                    instance = storage.AddComponent<PropertyStorage>();
                }

                return instance;
            }
        }

        public void UnmarkAll()
        {
            rowChanged = false;
            beatModifierSyncoChanged = false;
            changedProperties.Clear();
        }

        public void MarkChanged(Property property)
        {
            changedProperties.Add(property);
        }

        public bool HasChanged(Property property)
        {
            return changedProperties.Contains(property);
        }

        public void AddRowPropertyControl(PropertyControl_Row propertyControl)
        {
            rowPropertyControls.Add(propertyControl);
        }

        public void UpdateRowPropertyControls()
        {
            foreach (var control in rowPropertyControls)
            {
                control.SetDropdownOptionsUsingRows(control.controlAttribute.includeAll);
            }
        }

        public void ScheduleRowPropertyControlsUpdate()
        {
            StartCoroutine(RowPropertyControlsLateUpdate());
        }

        private IEnumerator RowPropertyControlsLateUpdate()
        {
            // I have no clue why I have to wait here LOL
            yield return new WaitForEndOfFrame();
            UpdateRowPropertyControls();
        }

        public bool rowChanged = false;
        public bool colorPropertyEqual = false;
        public bool colorChanged = false;
        public bool skipUpdatingPropertyUI = false;
        public bool scrollToTopOnUpdate = true;
        public bool beatModifierSyncoChanged = false;

        private readonly List<PropertyControl_Row> rowPropertyControls = new();
        private readonly HashSet<Property> changedProperties = new();
    }
}
