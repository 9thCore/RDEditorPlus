using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.LevelOptions.CustomClass
{
    internal static class Patch_InspectorPanel_LevelSettings
    {
        static Patch_InspectorPanel_LevelSettings()
        {
            string path = FileUtil.GetFilePathFromAssembly(Filename);
            if (!File.Exists(path))
            {
                allowedCustomClasses = DefaultAllowedCustomClasses;
            }
            else
            {
                try
                {
                    allowedCustomClasses = [.. File.ReadAllLines(path).Where(line => !line.IsNullOrEmpty() && line != EmptyCustomClass).Distinct()];
                    allowedCustomClasses.Insert(0, EmptyCustomClass);
                }
                catch (Exception exception)
                {
                    Plugin.LogError($"Tried to read from {path}, but was met with {exception} instead. Using default custom class list.");
                    allowedCustomClasses = DefaultAllowedCustomClasses;
                }
            }

            allowedCustomClassesCount = allowedCustomClasses.Count;
        }

        [HarmonyPatch(typeof(InspectorPanel_LevelSettings), nameof(InspectorPanel_LevelSettings.Awake))]
        private static class Awake
        {
            private static void Postfix(InspectorPanel_LevelSettings __instance)
            {
                UnityUtil.CreateDropdown(__instance.general.transform, out var dropdown, out var dropdownRT);

                dropdown.ClearOptions();
                dropdown.AddOptions(allowedCustomClasses);
                dropdown.onValueChanged.AddListener(OnSelectItem);

                dropdownRT.anchorMin = Vector2.zero;
                dropdownRT.anchorMax = Vector2.one;
                dropdownRT.offsetMin = new Vector2(TextEndPoint, CutFactor);
                dropdownRT.offsetMax = new Vector2(TotalWidth, 0f);
                dropdownRT.AnchorPosY(-__instance.generalHeight + ExtraVerticalOffset);

                var textGO = GameObject.Instantiate(__instance.totalHits.gameObject, __instance.general.transform);

                var text = textGO.GetComponent<Text>();
                text.text = "Custom Class:";

                var textRT = textGO.transform as RectTransform;
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = new Vector2(7f, CutFactor);
                textRT.offsetMax = new Vector2(TotalWidth - TextEndPoint, 0f);
                textRT.AnchorPosY(-__instance.generalHeight + ExtraVerticalOffset);

                __instance.generalHeight += ExtraVerticalOffset;

                Patch_InspectorPanel_LevelSettings.dropdown = dropdown;
            }
        }

        [HarmonyPatch(typeof(InspectorPanel_LevelSettings), nameof(InspectorPanel_LevelSettings.UpdateUI))]
        private static class UpdateUI
        {
            private static void Postfix() => UpdateDropdownUI(RDLevelData.current.settings.customClass);
        }

        private static void OnSelectItem(int index)
        {
            const string sound = "sndEditorValueChange";
            const string group = "LevelEditorInspectorPanel";

            var manager = scnEditor.instance.inspectorPanelManager;
            manager.LevelEditorPlaySound(sound, group);

            string customClass = allowedCustomClasses[index];

            using (new SaveStateScope())
            {
                scnEditor.instance.levelSettings.customClass = customClass switch
                {
                    EmptyCustomClass => null,
                    _ => customClass.ToString()
                };
            }

            int oldValue = dropdown.value;
            ResetDropdownToUsualOptionsIfNeeded();
            dropdown.SetValueWithoutNotify(oldValue);
        }

        private static void UpdateDropdownUI(string customClass)
        {
            if (customClass.IsNullOrEmpty())
            {
                dropdown.SetValueWithoutNotify(0);
                return;
            }

            int index = allowedCustomClasses.IndexOf(customClass);
            if (index != -1)
            {
                ResetDropdownToUsualOptionsIfNeeded();
                dropdown.SetValueWithoutNotify(index);
                return;
            }

            dropdown.ClearOptions();

            var list = AllowedCustomClassesWithBadExtra(customClass);
            dropdown.AddOptions(list);
            dropdown.SetValueWithoutNotify(list.Count - 1);
        }

        private static void ResetDropdownToUsualOptionsIfNeeded()
        {
            if (dropdown.options.Count == allowedCustomClassesCount)
            {
                return;
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(allowedCustomClasses);
        }

        private static Dropdown dropdown;

        private static readonly List<string> allowedCustomClasses;
        private static readonly int allowedCustomClassesCount;

        private static List<string> DefaultAllowedCustomClasses => [EmptyCustomClass, "Injury", "Intimate", "LuckyBreak", "PaigesReckoning",
            "Rollerdisco", "Unbeatable", "VividStasis", "Unreachable", "Freezeshot", "EdegaPerformance", "DistantDuet", "CareLess"];

        private static List<string> AllowedCustomClassesWithBadExtra(string key)
            => [.. allowedCustomClasses, $"<color=#800000>{key}</color>"];

        private const float ExtraVerticalOffset = 25f;
        private const float TextEndPoint = 70f;
        private const float TotalWidth = 114f;
        private const float CutFactor = 35f;
        private const string EmptyCustomClass = "None";

        public const string Filename = "customClasses.txt";
    }
}
