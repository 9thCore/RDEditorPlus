using System;
using System.Reflection;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Attributes
{
    public class VariableTypeAttribute(Type type) : Attribute
    {
        public GameObject Prefab
        {
            get
            {
                if (prefab == null)
                {
                    prefab = type
                        .GetMethod("get_VariablePrefab", BindingFlags.Public | BindingFlags.Static)
                        .Invoke(null, []) as GameObject;
                }

                return prefab;
            }
        }

        private GameObject prefab;
    }

    public class VariableTypeAttribute<T>() : VariableTypeAttribute(typeof(T)) { }
}
