using RDEditorPlus.Functionality.Mixins;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace RDEditorPlus.Util
{
    public static class AutocompletionStorageUtil
    {
        public static void StartFetch(this IAutocompletionStorage storage)
        {
            (storage as MonoBehaviour).StartCoroutine(storage.Fetch());
        }

        public static IEnumerator Fetch(this IAutocompletionStorage storage)
        {
            switch (storage.Behaviour)
            {
                case PluginConfig.AutocompleteBehaviour.Disabled:
                    yield break;
                case PluginConfig.AutocompleteBehaviour.RequestFromWeb:
                    yield return FetchFromWeb(storage);
                    yield break;
                case PluginConfig.AutocompleteBehaviour.FetchFromFile:
                    yield return FetchFromFile(storage);
                    yield break;
            }
        }

        private static IEnumerator FetchFromWeb(IAutocompletionStorage storage)
        {
            string file = GetTemporaryAutodownloadFileLocation(storage);
            if (File.Exists(file) && (DateTime.Now - File.GetLastWriteTime(file) < TimeSpan.FromDays(storage.RefreshTime)))
            {
                yield return storage.Parse(File.ReadAllText(file));
                yield break;
            }

            using UnityWebRequest webRequest = UnityWebRequest.Get(storage.URL);
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Plugin.LogError($"({storage.Identifier}) web request error: {webRequest.error}");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Plugin.LogError($"({storage.Identifier}) web request HTTP Error: {webRequest.error}");
                    break;
                case UnityWebRequest.Result.Success:
                    try
                    {
                        File.WriteAllText(file, webRequest.downloadHandler.text);
                        Plugin.LogInfo($"({storage.Identifier}) Saved temporary autocompletion data to {storage.TemporaryAutodownloadFile}.");
                    }
                    catch { }

                    yield return storage.Parse(webRequest.downloadHandler.text);
                    break;
            }
        }

        private static IEnumerator FetchFromFile(IAutocompletionStorage storage)
        {
            string file = GetPlayerSuppliedFileLocation(storage);

            if (!File.Exists(file))
            {
                Plugin.LogError($"({storage.Identifier}) File {storage.PlayerSuppliedFile} does not exist, cannot fetch autocompletion data.");
                yield break;
            }

            yield return storage.Parse(File.ReadAllText(file));
        }

        private static string GetPlayerSuppliedFileLocation(IAutocompletionStorage storage)
            => FileUtil.GetFilePathFromAssembly(storage.PlayerSuppliedFile);

        private static string GetTemporaryAutodownloadFileLocation(IAutocompletionStorage storage)
            => FileUtil.GetFilePathFromAssembly(storage.TemporaryAutodownloadFile);
    }
}
