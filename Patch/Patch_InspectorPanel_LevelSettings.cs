using HarmonyLib;
using RDEditorPlus.Functionality.General;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch
{
    internal static class Patch_InspectorPanel_LevelSettings
    {
        [HarmonyPatch(typeof(InspectorPanel_LevelSettings), nameof(InspectorPanel_LevelSettings.Awake))]
        private static class Awake
        {
            private static void Postfix(InspectorPanel_LevelSettings __instance)
            {
                foreach (var registration in SettingsInspectorRegistry.Settings)
                {
                    float anchorPosY = -__instance.generalHeight + VerticalSpacing + VerticalOffset;
                    Vector2 offsetMin = new(TextEndPoint, CutFactor);
                    Vector2 offsetMax = new(TotalWidth, 0f);

                    var textGO = GameObject.Instantiate(__instance.totalHits.gameObject, __instance.general.transform);

                    var text = textGO.GetComponent<Text>();
                    text.text = registration.Name + ":";
                    text.raycastTarget = false;

                    var textRT = textGO.transform as RectTransform;
                    textRT.anchorMin = Vector2.zero;
                    textRT.anchorMax = Vector2.one;
                    textRT.offsetMin = new Vector2(7f, CutFactor);
                    textRT.offsetMax = new Vector2(TotalWidth - TextEndPoint, 0f);
                    textRT.AnchorPosY(anchorPosY);

                    registration.Setup(__instance.general.transform, offsetMin, offsetMax, anchorPosY);

                    __instance.generalHeight += VerticalSpacing;
                }
            }
        }

        [HarmonyPatch(typeof(InspectorPanel_LevelSettings), nameof(InspectorPanel_LevelSettings.UpdateUI))]
        private static class UpdateUI
        {
            private static void Postfix() => SettingsInspectorRegistry.OnUpdateUI?.Invoke();
        }

        private const float VerticalOffset = 7f;
        private const float VerticalSpacing = 18f;
        private const float TextEndPoint = 70f;
        private const float TotalWidth = 114f;
        private const float CutFactor = 35f;
    }
}
