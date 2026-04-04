using HarmonyLib;
using RDEditorPlus.Functionality.LevelOptions.Mods;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.LevelOptions.Mods
{
    internal static class Patch_InspectorPanel_LevelSettings
    {
        [HarmonyPatch(typeof(InspectorPanel_LevelSettings), nameof(InspectorPanel_LevelSettings.Awake))]
        private static class Awake
        {
            private static void Postfix(InspectorPanel_LevelSettings __instance)
            {
                GameObject buttonGO = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_ModsButton");

                var buttonImage = buttonGO.AddComponent<Image>();
                buttonImage.sprite = AssetUtil.ButtonSprite;
                buttonImage.type = Image.Type.Tiled;
                buttonImage.color = Color.gray;

                var button = buttonGO.AddComponent<Button>();
                button.onClick.AddListener(OnClick);

                var buttonRT = buttonGO.transform as RectTransform;
                buttonRT.SetParent(__instance.general.transform, worldPositionStays: false);
                buttonRT.anchorMin = Vector2.zero;
                buttonRT.anchorMax = Vector2.one;
                buttonRT.offsetMin = new Vector2(TextEndPoint, CutFactor);
                buttonRT.offsetMax = new Vector2(TotalWidth, 0f);
                buttonRT.AnchorPosY(-__instance.generalHeight + ExtraVerticalOffset + 17f);

                GameObject buttonTextGO = new("test");

                var buttonText = buttonTextGO.AddComponent<Text>();
                buttonText.text = "Setup...";
                buttonText.ApplyRDFont();
                buttonText.alignment = TextAnchor.MiddleLeft;

                var buttonTextRT = buttonTextGO.transform as RectTransform;
                buttonTextRT.SetParent(buttonRT, worldPositionStays: false);
                buttonTextRT.anchorMin = Vector2.zero;
                buttonTextRT.anchorMax = Vector2.one;
                buttonTextRT.offsetMin = new Vector2(2f, 2f);
                buttonTextRT.offsetMax = new Vector2(-2f, -2f);

                var textGO = GameObject.Instantiate(__instance.totalHits.gameObject, __instance.general.transform);

                var text = textGO.GetComponent<Text>();
                text.text = "Mods:";
                text.raycastTarget = false;

                var textRT = textGO.transform as RectTransform;
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = new Vector2(7f, CutFactor);
                textRT.offsetMax = new Vector2(TotalWidth - TextEndPoint, 0f);
                textRT.AnchorPosY(-__instance.generalHeight + ExtraVerticalOffset);

                __instance.generalHeight += ExtraVerticalOffset;
            }
        }

        private static void OnClick() => ModPanelHolder.Instance.Toggle(show: true);

        private const float ExtraVerticalOffset = 25f;
        private const float TextEndPoint = 70f;
        private const float TotalWidth = 114f;
        private const float CutFactor = 35f;
    }
}
