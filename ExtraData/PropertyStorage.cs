
using RDLevelEditor;
using System.Collections.Generic;
using UnityEngine;

namespace RDEditorPlus.ExtraData
{
    internal class PropertyStorage
    {
        private static PropertyStorage instance;
        public static PropertyStorage Instance
        {
            get
            {
                instance ??= new();
                return instance;
            }
        }

        public void UnmarkAll()
        {
            changedProperties.Clear();
        }

        public void MarkChanged(Property property)
        {
            changedProperties.Add(property);
        }

        public bool HasChanged(Property property)
        {
            return changedProperties.Contains(property);
        }

        private readonly HashSet<Property> changedProperties = new();
    }
}
