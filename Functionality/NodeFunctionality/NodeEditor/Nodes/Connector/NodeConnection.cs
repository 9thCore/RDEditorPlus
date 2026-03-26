using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector
{
    public class NodeConnection : MonoBehaviour
    {
        public static NodeConnection Create(RectTransform parent)
        {
            GameObject instance = Instantiate(Prefab, parent);
            instance.SetActive(true);
            return instance.GetComponent<NodeConnection>();
        }

        public void SetStartColor(Color color) => graphic.StartColor = color;
        public void SetEndColor(Color color) => graphic.EndColor = color;
        public void SetColors(Color color) => graphic.SetColors(color);

        public void SetAnchor(RectTransform anchor)
        {
            rectTransform.position = anchor.position;
            rectTransform.localEulerAngles = Vector3.zero;
        }

        public void SetEndPoint(Vector2 point)
        {
            graphic.SetEndPoint(point - rectTransform.position.xy());
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private NodeConnectionGraphic graphic;

        private const float Height = 1f;

        private static GameObject Prefab
        {
            get
            {
                if (prefab == null)
                {
                    prefab = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(NodeConnection)}");
                    prefab.SetActive(false);

                    var prefabRT = prefab.AddComponent<RectTransform>();

                    var graphic = prefab.AddComponent<NodeConnectionGraphic>();
                    graphic.raycastTarget = false;

                    var connection = prefab.AddComponent<NodeConnection>();
                    connection.rectTransform = prefabRT;
                    connection.graphic = graphic;
                }

                return prefab;
            }
        }

        private static GameObject prefab;
    }
}
