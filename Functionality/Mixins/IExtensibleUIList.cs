using System.Collections.Generic;
using UnityEngine;

namespace RDEditorPlus.Functionality.Mixins
{
    public interface IExtensibleUIList<ListElement> where ListElement : IExtensibleUIList<ListElement>.IListElement
    {
        IList<ListElement> VariableList { get; }
        IListCreatorElement CreatorElement { get; }
        RectTransform ContentRectTransform { get; }

        ListElement CreateElement(int index);

        public interface IListElement
        {
            public bool Active { get; }
            public float Position { get; }

            public void SetActive(bool active);
            public void SetDownArrowVisibility(bool visible);
        }

        public interface IListCreatorElement
        {
            public void MoveTo(float position);
        }
    }
}
