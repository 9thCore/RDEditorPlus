using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Functionality.Components;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
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

                if (__instance.position.evTag != null)
                {
                    InspectorUtil.SetupMixedPlaceholder(__instance.position.evTag).color = Color.white.WithAlpha(InspectorUtil.MixedTextAlpha);

                    __instance.position.evTag.onEndEdit.AddListener(text =>
                    {
                        if (!InspectorUtil.CanMultiEdit()
                        || string.IsNullOrEmpty(text))
                        {
                            return;
                        }

                        __instance.position.evTag.SetTextWithoutNotify(text);

                        ((Text)__instance.position.beat.placeholder).text = string.Empty;
                        foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                        {
                            eventControl.levelEvent.tag = text;
                        }
                    });
                }

                if (__instance.position.evTagRunToggle != null)
                {
                    __instance.position.evTagRunToggle.onValueChanged.AddListener(value =>
                    {
                        if (!InspectorUtil.CanMultiEdit())
                        {
                            return;
                        }

                        foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
                        {
                            eventControl.levelEvent.tagRunNormally = value;
                        }

                        __instance.position.MultiEditUpdateUI();
                        __instance.position.evTagRunToggle.isOn = value;
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

        [HarmonyPatch(typeof(InspectorPanel), nameof(InspectorPanel.AwakeAuto))]
        private static class AwakeAuto
        {
            private static void Postfix(InspectorPanel __instance)
            {
                if (__instance.levelEventInfo.showsRowControl)
                {
                    RowCustomDropdown customDropdown = __instance.row.dropdown.ReplaceWithDerivative<RowCustomDropdown>();
                    __instance.row.dropdown = customDropdown;
                }
            }
        }

        [HarmonyPatch(typeof(InspectorPanel), nameof(InspectorPanel.UpdateUIAuto))]
        private static class UpdateUIAuto
        {
            private static void Postfix(InspectorPanel __instance)
            {
                if (!__instance.RowEqualValueForSelectedEvents())
                {
                    __instance.row.dropdown.captionText.text = InspectorUtil.MixedText;
                }
            }
        }

        [HarmonyPatch(typeof(InspectorPanel), nameof(InspectorPanel.UpdateUI))]
        private static class UpdateUI
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                ILLabel label = null;
                cursor
                    .GotoNext(instruction => instruction.MatchLdfld<InspectorPanel>(nameof(InspectorPanel.position)))
                    .GotoNext(MoveType.After, instruction => instruction.MatchBrfalse(out label))
                    .Emit(OpCodes.Ldarg_0)
                    .EmitDelegate(InspectorUtil.TryMultiEditUpdateUI);

                cursor
                    .Emit(OpCodes.Brfalse, label);
            }
        }
    }
}
