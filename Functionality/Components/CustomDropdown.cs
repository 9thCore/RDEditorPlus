using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.Components
{
    // this sucks
    public class CustomDropdown : Dropdown
    {
        protected override GameObject CreateDropdownList(GameObject template)
        {
            itemIndex = 0;
            currentlyCreatingDropdown = true;
            return base.CreateDropdownList(template);
        }

        protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            DropdownItem item = base.CreateItem(itemTemplate);

            if (!IsOfEqualValue)
            {
                int valueClearlyDifferentFromIndex = (itemIndex == 0) ? 1 : 0;
                item.toggle.onValueChanged.AddListener(_ =>
                {
                    if (currentlyCreatingDropdown)
                    {
                        return;
                    }

                    // Ensure the dropdown's value is definitely changed, even if it's the same as before
                    SetValueWithoutNotify(valueClearlyDifferentFromIndex);
                });
            }

            itemIndex++;
            return item;
        }

        protected override GameObject CreateBlocker(Canvas rootCanvas)
        {
            GameObject blocker = base.CreateBlocker(rootCanvas);

            if (!IsOfEqualValue)
            {
                captionText.text = InspectorUtil.MixedText;

                foreach (Toggle toggle in GetComponentsInChildren<Toggle>())
                {
                    toggle.SetIsOnWithoutNotify(false);
                }
            }

            currentlyCreatingDropdown = false;
            return blocker;
        }

        protected virtual bool IsOfEqualValue => !this.TryGetComponentInParent(out Property property)
            || property.control.EqualValueForSelectedEvents();

        private bool currentlyCreatingDropdown;
        private int itemIndex;
    }
}
