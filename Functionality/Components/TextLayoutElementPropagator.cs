using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.Components
{
    public class TextLayoutElementPropagator : LayoutElement
    {
        public TextLayoutElementPropagator Setup(Text element)
        {
            this.element = element;
            return this;
        }

        public TextLayoutElementPropagator SetHeightPadding(float height)
        {
            heightPadding = height;
            return this;
        }

        public override float minHeight => element.minHeight + heightPadding;
        public override float preferredHeight => element.preferredHeight + heightPadding;

        [SerializeField]
        private Text element = null;
        [SerializeField]
        private float heightPadding = 0f;
    }
}
