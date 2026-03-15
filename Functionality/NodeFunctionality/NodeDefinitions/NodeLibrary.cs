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
                if (instance == null || !instance.Valid())
                {
                    instance = new();
                }

                return instance;
            }
        }

        public void Prime()
        {
            // No-op, used to prime the prefabs to be ready
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

        private bool Valid() => nodeData[aNode].Prefab != null;

        private NodeLibrary()
        {
            var assembly = typeof(NodeLibrary).Assembly;

            var nodeTypes = assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(Node_Base)) && !type.IsAbstract);

            foreach (var type in nodeTypes)
            {
                var name = type.Name.Substring(startIndex: 5);
                var method = type.GetMethod(nameof(Node_Base.PreparePrefab), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                var prefab = (GameObject)method.Invoke(null, [name]);
                nodeData.Add(name, new(prefab, type));
                aNode = name;
            }
        }

        private readonly Dictionary<string, NodeData> nodeData = new();
        private readonly string aNode;

        private record NodeData(GameObject Prefab, Type Type);
    }
}
