using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Util
{
    public static class TabUtil
    {
        public static void SetupWithScrollMaskIntermediary(this RectTransform rectTransform, string nameSuffix)
        {
            if (rectTransform.parent.TryGetComponent(out Mask _))
            {
                return;
            }

            GameObject mask = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameSuffix}");

            mask.EnsureComponent<Image>();
            mask.EnsureComponent<Mask>().showMaskGraphic = false;
            mask.layer = rectTransform.gameObject.layer;

            int siblingIndex = rectTransform.GetSiblingIndex();

            RectTransform maskTransform = mask.GetComponent<RectTransform>();
            maskTransform.SetParent(rectTransform.parent);
            rectTransform.SetParent(maskTransform);

            maskTransform.SetSiblingIndex(siblingIndex);

            maskTransform.anchorMin = Vector2.zero;
            maskTransform.anchorMax = Vector2.one;
            maskTransform.offsetMin = new Vector2(2f, 0f);
            maskTransform.offsetMax = new Vector2(-1f, -16f);
        }
    }
}
