using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_Conditionals
    {
        [HarmonyPatch(typeof(Conditionals), nameof(Conditionals.ShowListPanel))]
        private static class ShowListPanel
        {
            private static void Postfix(Conditionals __instance, bool visible)
            {
                if (!visible || !InspectorUtil.CanMultiEdit())
                {
                    return;
                }

                foreach (ConditionalInfo info in scnEditor.instance.allConditionals.Zip(__instance.conditionalButtons, (c, b) => new ConditionalInfo(b, c)))
                {
                    ConditionalUtil.MultiEditUsageType multiEditUsageType = default;
                    ConditionalUtil.UsageType usageType = default;

                    multiEditUsageType = info.Conditional.gid.IsNullOrEmpty()
                        ? ConditionalUtil.IsUsedMultiEdit(info.Conditional.id, out usageType)
                        : ConditionalUtil.IsUsedMultiEdit(info.Conditional.gid, out usageType);

                    Plugin.LogInfo($"Usage: {multiEditUsageType}, {usageType}");

                    info.Button.Selected = multiEditUsageType == ConditionalUtil.MultiEditUsageType.UsedByAll;
                    info.Button.Negated = usageType == ConditionalUtil.UsageType.Negated;

                    Color color = usageType switch
                    {
                        ConditionalUtil.UsageType.Normal => InspectorUtil.ConditionalNormalColor,
                        ConditionalUtil.UsageType.Negated => InspectorUtil.ConditionalNegatedColor,
                        ConditionalUtil.UsageType.Mixed => InspectorUtil.ConditionalMixedColor,
                        _ => InspectorUtil.ConditionalUnusedColor
                    };
                    
                    if (multiEditUsageType == ConditionalUtil.MultiEditUsageType.UsedBySome)
                    {
                        info.Button.outline.SetActive(true);

                        GameObject parent = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_ConditionalButtonMask");
                        Image image = parent.AddComponent<Image>();
                        image.type = Image.Type.Tiled;
                        image.sprite = ConditionalUtil.UsedBySomeMaskSprite;
                        image.raycastTarget = false;

                        Mask mask = parent.AddComponent<Mask>();
                        mask.showMaskGraphic = false;

                        RectTransform transform = parent.transform as RectTransform;
                        RectTransform imageTransform = info.Button.outlineImage.transform as RectTransform;

                        transform.SetParent(imageTransform.parent);
                        transform.localPosition = Vector3.zero;
                        transform.localScale = Vector3.one;
                        transform.localRotation = Quaternion.identity;
                        transform.anchorMin = imageTransform.anchorMin;
                        transform.anchorMax = imageTransform.anchorMax;
                        transform.offsetMin = Vector2.zero;
                        transform.offsetMax = Vector2.zero;

                        imageTransform.SetParent(transform);

                        info.Button.conditional.onClick.AddListener(() =>
                        {
                            image.enabled = false;
                            mask.enabled = false;
                        });
                    }

                    info.Button.outlineImage.color = color;
                }
            }

            private record struct ConditionalInfo(ConditionalButton Button, Conditional Conditional);
        }
    }
}
