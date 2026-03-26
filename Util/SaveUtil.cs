using UnityEngine;

namespace RDEditorPlus.Util
{
    public static class SaveUtil
    {
        public static string GetString(string key, string defaultValue) => PlayerPrefs.GetString(GetKey(key), defaultValue);
        public static void SetString(string key, string value) => PlayerPrefs.SetString(GetKey(key), value);

        private static string GetKey(string key) => $"{KeyPrefix}_{key}";

        private const string KeyPrefix = $"Mod_{MyPluginInfo.PLUGIN_GUID}_";
    }
}
