using DG.Tweening;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.ArbitraryPanel
{
    public abstract class ArbitraryPanelHolder
    {
        public abstract void OnShow();
        public abstract void OnHide();

        public void Toggle(bool show)
        {
            if (show)
            {
                gameObject.SetActive(true);
                OnShow();
            }
            else
            {
                OnHide();
            }

            Vector3 start = show ? Vector3.zero : DesiredScale;
            Vector3 end = show ? DesiredScale : Vector3.zero;

            float time = show ? RDConstants.data.roomsSelectionPopup_showAnimDuration : RDConstants.data.roomsSelectionPopup_hideAnimDuration;
            Ease ease = show ? RDConstants.data.roomsSelectionPopup_showAnimEase : RDConstants.data.roomsSelectionPopup_hideAnimEase;

            rectTransform.DOKill(true);
            rectTransform.localScale = start;
            rectTransform.DOScale(end, time).SetEase(ease).SetUpdate(UpdateType.Normal, true)
                .OnComplete(delegate
                {
                    if (!show)
                    {
                        gameObject.SetActive(false);
                    }
                });
        }

        public void Toggle() => Toggle(!gameObject.activeSelf);

        public ArbitraryPanelHolder()
        {
            var template = scnEditor.instance.publishPopup.gameObject;
            var templateTransform = template.transform;

            var clone = Object.Instantiate(template);
            clone.name = $"Mod_{MyPluginInfo.PLUGIN_GUID}_{GetType().Name}";

            var offset = new Vector2(16f, 10f);

            var transform = clone.transform as RectTransform;
            transform.offsetMin -= offset;
            transform.offsetMax += offset;

            transform.SetParent(templateTransform.parent);
            transform.localPosition = templateTransform.localPosition;
            transform.localRotation = templateTransform.localRotation;
            transform.localScale = templateTransform.localScale;

            (transform.Find("Image").transform as RectTransform).anchoredPosition += offset;

            var popup = clone.GetComponent<RDPublishPopup>();
            popup.enabled = false;

            popup.onDialogScreenBlocker.raycastTarget = true;
            popup.onDialogScreenBlocker.transform.SetAsFirstSibling();

            grayButtonClone = Object.Instantiate(popup.steamPublishButton.gameObject);
            grayButtonClone.SetActive(false);

            var button2 = grayButtonClone.GetComponent<Button>();
            grayButtonCloneColorBlock = button2.colors;
            Object.DestroyImmediate(button2);
            Object.DestroyImmediate(grayButtonClone.GetComponentInChildren<RDStringToUIText>());

            Object.DestroyImmediate(popup.steamPublishButton.transform.parent.gameObject);
            Object.DestroyImmediate(popup.levelErrorPresentation.gameObject);
            Object.DestroyImmediate(popup.steamUpdateContainer.gameObject);
            Object.DestroyImmediate(popup.levelDataContainer.gameObject);
            Object.DestroyImmediate(popup.incompleteSettingsText.gameObject);
            Object.DestroyImmediate(popup);

            title = clone.GetComponentInChildren<Text>();
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.offsetMin = new Vector2(0f, -10f);
            title.rectTransform.offsetMax = Vector2.zero;
            Object.DestroyImmediate(title.GetComponent<RDStringToUIText>());

            var button = clone.GetComponentInChildren<Button>();
            var buttonObject = button.gameObject;
            var colors = button.colors;
            Object.DestroyImmediate(button);

            button = buttonObject.AddComponent<Button>();
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                Toggle(show: false);
            });

            gameObject = clone;
            rectTransform = transform;
        }

        protected readonly GameObject gameObject;
        protected readonly RectTransform rectTransform;
        protected readonly Text title;

        protected bool Valid() => gameObject != null;

        protected Vector3 DesiredScale
        {
            get
            {
                float windowScale = Screen.width / scrVfxControl.CanvasWidth;
                float editorScale = scnEditor.instance.canvasScaler.scaleFactor;
                float scale = windowScale / editorScale;

                return Vector3.one * scale;
            }
        }

        protected static GameObject CloneButton(Transform parent, string name, UnityAction call, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject clone = Object.Instantiate(grayButtonClone, parent);
            var button = clone.AddComponent<Button>();
            button.colors = grayButtonCloneColorBlock;
            button.onClick.AddListener(call);

            clone.GetComponentInChildren<Text>().text = name;

            var rt = clone.transform as RectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            clone.SetActive(true);
            return clone;
        }

        protected static GameObject grayButtonClone;
        protected static ColorBlock grayButtonCloneColorBlock;
    }
}
