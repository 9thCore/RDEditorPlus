using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Util
{
    public static class AssetUtil
    {
        [Filename("editor-inputfield-2px")]
        public static Sprite InputFieldSprite { get; private set; }

        [Filename("editor-button-2px")]
        public static Sprite ButtonSprite { get; private set; }

        [Filename("editor_browse1")]
        public static Sprite Browse1Sprite { get; private set; }

        [Filename("editor-checkmark")]
        public static Sprite CheckmarkSprite { get; private set; }

        [Filename("RDLatinFontPoint")]
        public static Font StandardFont { get; private set; }

        public static void ApplyRDFont(this Text text)
        {
            text.font = StandardFont;
            text.fontSize = 8;
        }

        public static void FetchEditorAssets()
        {
            if (fetched)
            {
                return;
            }

            fetched = true;

            FetchEditorAssets<Sprite>();
            FetchEditorAssets<Font>();
        }

        private static void FetchEditorAssets<T>() where T : UnityEngine.Object
        {
            var definitions = GetAssetDefinitions<T>();
            var assets = Resources.FindObjectsOfTypeAll<T>();

            foreach (var asset in assets)
            {
                if (definitions.TryGetValue(asset.name, out var definition))
                {
                    definition.Property.SetValue(null, asset);
                }
            }
        }

        private static Dictionary<string, AssetDefinition> GetAssetDefinitions<T>()
        {
            var properties = typeof(AssetUtil).GetProperties();
            return properties
                .Where(property => property.PropertyType == typeof(T))
                .Select(property => new AssetDefinition(property, property.GetCustomAttribute<FilenameAttribute>()))
                .ToDictionary(property => property.Name);
        }

        private static bool fetched = false;

        private record AssetDefinition(PropertyInfo Property, FilenameAttribute Attribute)
        {
            public string Name => Attribute.Name;
        }

        [AttributeUsage(AttributeTargets.Property)]
        private class FilenameAttribute(string name) : Attribute
        {
            public readonly string Name = name;
        }
    }
}
