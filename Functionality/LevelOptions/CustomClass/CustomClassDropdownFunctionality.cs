using RDEditorPlus.Functionality.General;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.LevelOptions.CustomClass
{
    public static class CustomClassDropdownFunctionality
    {
        static CustomClassDropdownFunctionality()
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

        public static void UpdateUI() => UpdateDropdownUI(scnEditor.instance.levelSettings.customClass);

        public static void Register()
        {
            option = new("Custom Class", allowedCustomClasses, OnSelectItem);
            SettingsInspectorRegistry.Register(option);
            SettingsInspectorRegistry.OnUpdateUI += UpdateUI;
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

            int oldValue = Dropdown.value;
            ResetDropdownToUsualOptionsIfNeeded();
            Dropdown.SetValueWithoutNotify(oldValue);
        }

        private static void UpdateDropdownUI(string customClass)
        {
            if (customClass.IsNullOrEmpty())
            {
                Dropdown.SetValueWithoutNotify(0);
                return;
            }

            int index = allowedCustomClasses.IndexOf(customClass);
            if (index != -1)
            {
                ResetDropdownToUsualOptionsIfNeeded();
                Dropdown.SetValueWithoutNotify(index);
                return;
            }

            Dropdown.ClearOptions();

            var list = AllowedCustomClassesWithBadExtra(customClass);
            Dropdown.AddOptions(list);
            Dropdown.SetValueWithoutNotify(list.Count - 1);
        }

        private static void ResetDropdownToUsualOptionsIfNeeded()
        {
            if (Dropdown.options.Count == allowedCustomClassesCount)
            {
                return;
            }

            Dropdown.ClearOptions();
            Dropdown.AddOptions(allowedCustomClasses);
        }

        private static Dropdown Dropdown => option.Dropdown;
        private static SettingsInspectorRegistry.DropdownOption option;

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
