using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.Components
{
    public class TextLayoutElementPropagator : LayoutElement
    {
        public void Setup(Text element)
        {
            this.element = element;
        }

        public override float minHeight => element.minHeight;
        public override float preferredHeight => element.preferredHeight;

        [SerializeField]
        private Text element = null;
    }
}
