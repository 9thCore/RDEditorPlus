using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_InspectorPanel
    {
        [HarmonyPatch(typeof(InspectorPanel), nameof(InspectorPanel.Awake))]
        private static class Awake
        {
            private static void Postfix(InspectorPanel __instance)
            {
                if (__instance.position == null)
                {
                    return;
                }

                if (__instance.position.bar != null)
                {
                    InspectorUtil.SetupMixedPlaceholder(__instance.position.bar).color = Color.white.WithAlpha(InspectorUtil.MixedTextAlpha);

                    __instance.position.bar.onEndEdit.AddListener(text =>
                    {
                        if (!InspectorUtil.CanMultiEdit()
                        || !int.TryParse(text, out int bar))
                        {
                            return;
                        }

                        if (bar < 1)
                        {
                            bar = 1;
                            __instance.position.bar.SetTextWithoutNotify(bar.ToString());
                        }

                        ((Text)__instance.position.bar.placeholder).text = string.Empty;
                        foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                        {
                            eventControl.bar = bar;
                        }
                    });
                }

                if (__instance.position.beat != null)
                {
                    InspectorUtil.SetupMixedPlaceholder(__instance.position.beat).color = Color.white.WithAlpha(InspectorUtil.MixedTextAlpha);

                    __instance.position.beat.onEndEdit.AddListener(text =>
                    {
                        if (!InspectorUtil.CanMultiEdit()
                        || !float.TryParse(text, out float beat))
                        {
                            return;
                        }

                        if (beat < 1f)
                        {
                            beat = 1f;
                            __instance.position.beat.SetTextWithoutNotify(beat.ToString());
                        }

                        ((Text)__instance.position.beat.placeholder).text = string.Empty;
                        foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                        {
                            eventControl.beat = beat;
                        }
                    });
                }
            }
        }

        [HarmonyPatch(typeof(InspectorPanel), nameof(InspectorPanel.Show))]
        private static class Show
        {
            private static void Prefix()
            {
                PropertyStorage.Instance.UnmarkAll();
            }
        }
    }
}
