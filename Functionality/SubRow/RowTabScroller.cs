using UnityEngine;

namespace RDEditorPlus.Functionality.SubRow
{
    public class RowTabScroller : MonoBehaviour
    {
        public void LateUpdate()
        {
            RowManager.Instance.UpdateTabScroll();
        }
    }
}
