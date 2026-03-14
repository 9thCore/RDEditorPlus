using RDEditorPlus.Util;
using UnityEngine;
using UnityEngine.UI;

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

        public void SetAnchor(RectTransform anchor)
        {
            rectTransform.position = anchor.position;
            rectTransform.localEulerAngles = Vector3.zero;
            line.localPosition = Vector3.zero;
        }

        public void SetEndPoint(Vector2 point)
        {
            endPoint = point;
            Vector2 delta = point - rectTransform.position.xy();
            rectTransform.localEulerAngles = new(0f, 0f, -delta.GetAngle());
            line.sizeDelta = new Vector2(delta.magnitude / rectTransform.lossyScale.x, line.sizeDelta.y);
        }

        public void OffsetEndPoint(Vector2 delta)
        {
            SetEndPoint(endPoint + delta);
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        private Vector2 endPoint;

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private RectTransform line;

        private const float Height = 1f;

        private static GameObject Prefab
        {
            get
            {
                if (prefab == null)
                {
                    prefab = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_{nameof(NodeConnection)}");
                    prefab.SetActive(false);

                    GameObject line = new("line");

                    var rt2 = prefab.AddComponent<RectTransform>();
                    rt2.pivot = new Vector2(0f, 0.5f);

                    var image = line.AddComponent<Image>();
                    image.raycastTarget = false;

                    var rt = line.transform as RectTransform;
                    rt.SetParent(rt2);
                    rt.offsetMin = new Vector2(0f, -Height);
                    rt.offsetMax = new Vector2(20f, Height);
                    rt.pivot = new Vector2(0f, 0.5f);

                    var connection = prefab.AddComponent<NodeConnection>();
                    connection.rectTransform = rt2;
                    connection.line = rt;
                }

                return prefab;
            }
        }

        private static GameObject prefab;

        private static readonly Vector3[] WorldCornerStorage = new Vector3[4];
    }
}
