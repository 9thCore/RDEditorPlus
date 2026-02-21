using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace RDEditorPlus.ExtraData
{
    public class CustomMethodStorage : MonoBehaviour
    {
        private static CustomMethodStorage instance;
        public static CustomMethodStorage Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject manager = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_CustomMethodStorage");
                    instance = manager.AddComponent<CustomMethodStorage>();
                }

                return instance;
            }
        }

        public bool IsAllowed(MethodInfo methodInfo)
        {
            return nonDevAllowedMethods.Contains(methodInfo);
        }

        public bool TryGetDescription(string key, out string description)
        {
            if (key.IsNullOrEmpty())
            {
                description = default;
                return false;
            }

            return nonDevAllowedMethodDescriptions.TryGetValue(key, out description);
        }

        public void StartFetchOfMethods()
        {
            if (fetchedAlready)
            {
                return;
            }
            fetchedAlready = true;

            StartCoroutine(PrepareAllowedMethodsForFetch());
        }

        private IEnumerator PrepareAllowedMethodsForFetch()
        {
            switch (PluginConfig.CustomMethodsAutocomplete)
            {
                case PluginConfig.CustomMethodAutocompleteBehaviour.Disabled:
                    yield break;
                case PluginConfig.CustomMethodAutocompleteBehaviour.RequestFromWeb:

                    using (UnityWebRequest webRequest = UnityWebRequest.Get(CustomMethodsSpreadsheetURL))
                    {
                        yield return webRequest.SendWebRequest();

                        switch (webRequest.result)
                        {
                            case UnityWebRequest.Result.ConnectionError:
                            case UnityWebRequest.Result.DataProcessingError:
                                Plugin.LogError("Custom methods web request error: " + webRequest.error);
                                break;
                            case UnityWebRequest.Result.ProtocolError:
                                Plugin.LogError("Custom methods web request HTTP Error: " + webRequest.error);
                                break;
                            case UnityWebRequest.Result.Success:
                                yield return FetchMethodsFromText(webRequest.downloadHandler.text);
                                break;
                        }
                    }

                    yield break;
                case PluginConfig.CustomMethodAutocompleteBehaviour.FetchFromFile:
                    string fullPath = Assembly.GetExecutingAssembly().Location;
                    string directory = Path.GetDirectoryName(fullPath);
                    string file = Path.Combine(directory, CustomMethodsSpreadsheetFile);

                    if (File.Exists(file))
                    {
                        yield return FetchMethodsFromText(File.ReadAllText(file));
                    }
                    else
                    {
                        Plugin.LogError($"File {file} does not exist, cannot fetch custom method autocompletion data.");
                    }

                    yield break;
            }

            

            yield break;
        }

        private IEnumerator FetchMethodsFromText(string text)
        {
            // Only parse so much per frame
            const int MaxReadTimeout = 50;

            string customClass = string.Empty;
            int timeout = MaxReadTimeout;

            List<MethodInfo> collectedMethods = new();
            using StringReader stringReader = new(text);

            string line;
            while ((line = stringReader.ReadLine()) != null)
            {
                Match match = CustomClassMatcher.Match(line);
                if (match.Success)
                {
                    customClass = match.Groups[1].Value;
                }
                else if (TryFindMethod(customClass, line, out MethodInfo methodInfo))
                {
                    collectedMethods.Add(methodInfo);
                    nonDevAllowedMethods.Add(methodInfo);
                }
                else if (!line.StartsWith(RoomMethodPrefix) && !line.StartsWith(CommentCommandPrefix))
                {
                    int index = line.IndexOf('\t');
                    string description = ((index != -1) ? line.Substring(0, index) : line).Replace("  ", "\n\n");
                    string descriptionCapped = (description.Length < ArbitraryTextMaxLength) ? description : description.Substring(0, ArbitraryTextMaxLength);

                    foreach (var method in collectedMethods)
                    {
                        var key = $"customMethod.{GetMethodClass(method)}.{method.Name}";
                        nonDevAllowedMethodDescriptions[key] = descriptionCapped;
                    }

                    collectedMethods.Clear();
                }

                timeout--;

                if (timeout <= 0)
                {
                    timeout = MaxReadTimeout;
                    yield return null;
                }
            }

            yield break;
        }

        private bool TryFindMethod(string customClass, string unparsedName, out MethodInfo methodInfo)
        {
            if (unparsedName.StartsWith(CommentCommandPrefix))
            {
                // Comment command, ignore
                methodInfo = null;
                return false;
            }

            int parenthesisIndex = unparsedName.IndexOf('(');
            if (parenthesisIndex < 0)
            {
                methodInfo = null;
                return false;
            }

            int startIndex = 0;
            Type script = typeof(LevelBase);
            if (!string.IsNullOrEmpty(customClass)
                && Enum.TryParse(customClass, out Level level))
            {
                script = LevelSelector.GetLevelTypeFromEnum(level);
            }

            if (unparsedName.StartsWith(VfxMethodPrefix))
            {
                startIndex = VfxMethodPrefix.Length;
                script = typeof(scrVfxControl);
            }

            if (unparsedName.StartsWith(RoomMethodPrefix))
            {
                startIndex = unparsedName.IndexOf(']');
                script = typeof(RDRoom);

                if (startIndex < 0)
                {
                    methodInfo = null;
                    return false;
                }

                startIndex += 2; // Skip the ]. structure
            }

            string methodName = unparsedName.Substring(startIndex, parenthesisIndex - startIndex);
            methodInfo = script.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            return methodInfo != null;
        }

        private static string GetMethodClass(MethodInfo methodInfo)
        {
            Type type = methodInfo.DeclaringType;

            if (type == typeof(RDRoom))
            {
                return "room";
            }
            else if (type == typeof(scrVfxControl))
            {
                return "vfx";
            }
            else if (type == typeof(LevelBase)
                || type.IsSubclassOf(typeof(LevelBase)))
            {
                return "level";
            }

            return string.Empty;
        }

        private readonly HashSet<MethodInfo> nonDevAllowedMethods = new();
        private readonly Dictionary<string, string> nonDevAllowedMethodDescriptions = new();
        private bool fetchedAlready = false;

        private const string CommentCommandPrefix = "()=>";
        private const string VfxMethodPrefix = "vfx.";
        private const string RoomMethodPrefix = "room[";
        //private const string CustomClassPostfix = "Custom Methods";

        private readonly static Regex CustomClassMatcher = new("\"(.+)\" Custom Methods");

        public const string CustomMethodsSpreadsheetURL = "https://docs.google.com/spreadsheets/d/1JAz6iRLqcn08ZeTeBHeeDrpdX6M5K0b1qRVQomua21s/export?format=tsv&gid=1128429183";
        public const string CustomMethodsSpreadsheetFile = "customMethods.tsv";

        public const int ArbitraryTextMaxLength = 800;
    }
}
