using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
                    nonDevAllowedMethods.Add(methodInfo);
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

        private readonly HashSet<MethodInfo> nonDevAllowedMethods = new();
        private bool fetchedAlready = false;

        private const string CommentCommandPrefix = "()=>";
        private const string VfxMethodPrefix = "vfx.";
        private const string RoomMethodPrefix = "room[";
        //private const string CustomClassPostfix = "Custom Methods";

        private readonly static Regex CustomClassMatcher = new("\"(.+)\" Custom Methods");

        public const string CustomMethodsSpreadsheetURL = "https://docs.google.com/spreadsheets/d/1JAz6iRLqcn08ZeTeBHeeDrpdX6M5K0b1qRVQomua21s/export?format=tsv&gid=1128429183";
        public const string CustomMethodsSpreadsheetFile = "customMethods.tsv";
    }
}
