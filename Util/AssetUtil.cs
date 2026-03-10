using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RDEditorPlus.Util
{
    public static class AssetUtil
    {
        [Filename("editor-inputfield-2px")]
        public static Sprite InputFieldSprite { get; private set; }

        public static void FetchEditorAssets()
        {
            if (fetched)
            {
                return;
            }

            fetched = true;

            FetchEditorAssets<Sprite>();
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
