using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_Color
    {
        [HarmonyPatch(typeof(PropertyControl_Color), nameof(PropertyControl_Color.UpdateUI))]
        private static class UpdateUI
        {
            private static bool Prefix(PropertyControl_Color __instance)
            {
                if (!__instance.EqualValueForSelectedEvents())
                {
                    PluginConfig.MultiEditColorBehaviour behaviour = PluginConfig.SelectionMultiEditColorBehaviour;
                    bool averageAlpha = behaviour.HasFlag(PluginConfig.AverageAlphaFragment);

                    float a = 0f;
                    Color result = default;

                    int count = scnEditor.instance.selectedControls.Count;
                    switch (behaviour)
                    {
                        case PluginConfig.MultiEditColorBehaviour.JustSetToWhite:
                            result = Color.white;
                            break;
                        case PluginConfig.MultiEditColorBehaviour.AverageRGB:
                        case PluginConfig.MultiEditColorBehaviour.AverageRGBA:
                            float r = 0f, g = 0f, b = 0f;

                            foreach (var control in scnEditor.instance.selectedControls)
                            {
                                ColorOrPalette value = (ColorOrPalette)__instance.GetEventValue(control.levelEvent);
                                Color color = value.ToColor(useEditorPalette: true);
                                r += color.r;
                                g += color.g;
                                b += color.b;
                                a += color.a;
                            }

                            result = new(r / count, g / count, b / count);
                            break;
                        case PluginConfig.MultiEditColorBehaviour.AverageHSV:
                        case PluginConfig.MultiEditColorBehaviour.AverageHSVA:
                            float h = 0f, s = 0f, v = 0f;

                            foreach (var control in scnEditor.instance.selectedControls)
                            {
                                ColorOrPalette value = (ColorOrPalette)__instance.GetEventValue(control.levelEvent);
                                Color color = value.ToColor(useEditorPalette: true);
                                Color.RGBToHSV(color, out float deltaH, out float deltaS, out float deltaV);

                                h += deltaH;
                                s += deltaS;
                                v += deltaV;
                                a += color.a;
                            }

                            result = Color.HSVToRGB(h / count, s / count, v / count);
                            break;
                    }

                    __instance.colorPicker.colorOrPalette = new(result.WithAlpha(averageAlpha ? (a / count) : 1.0f));
                    PropertyStorage.Instance.colorPropertyEqual = false;
                    return false;
                }

                PropertyStorage.Instance.colorPropertyEqual = true;
                return true;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Color), nameof(PropertyControl_Color.Save))]
        private static class Save
        {
            private static bool Prefix()
            {
                return PropertyStorage.Instance.colorChanged;
            }
        }
    }
}
