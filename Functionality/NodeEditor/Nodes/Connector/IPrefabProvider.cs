using UnityEngine;

namespace RDEditorPlus.Functionality.NodeEditor.Nodes.Connector
{
    public interface IPrefabProvider
    {
        GameObject GetPrefab(Node.Type type);
    }
}
