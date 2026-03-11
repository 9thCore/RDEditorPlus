using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions
{
    public class NodeLibrary
    {
        private static NodeLibrary instance;
        public static NodeLibrary Instance
        {
            get
            {
                return instance ??= new();
            }
        }

        public GameObject GetPrefab(string name) => nodeData[name]?.Prefab;
        public bool TryGetInstance(string name, out Node_Base instance)
        {
            if (!nodeData.TryGetValue(name, out var data))
            {
                instance = default;
                return false;
            }

            instance = (Node_Base)Activator.CreateInstance(data.Type);
            return true;
        }

        private NodeLibrary()
        {
            var assembly = typeof(NodeLibrary).Assembly;

            var nodeTypes = assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(Node_Base)) && !type.IsAbstract);

            foreach (var type in nodeTypes)
            {
                var name = type.Name.Substring(startIndex: 5);
                var method = type.GetMethod(nameof(Node_Base.PreparePrefab), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                try
                {
                    var prefab = (GameObject)method.Invoke(null, [name]);
                    nodeData.Add(name, new(prefab, type));
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Could not cache type {type.FullName} because:\n{e}");
                }
            }
        }

        private readonly Dictionary<string, NodeData> nodeData = new();

        private record NodeData(GameObject Prefab, Type Type);
    }
}
