using System;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector
{
    public class NodeConnectionGraphic : MaskableGraphic
    {
        public float Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                SetVerticesDirty();
            }
        }

        public Color StartColor
        {
            get => startColor;
            set
            {
                startColor = value;
                SetVerticesDirty();
            }
        }

        public Color EndColor
        {
            get => endColor;
            set
            {
                endColor = value;
                SetVerticesDirty();
            }
        }

        public void SetColors(Color color)
        {
            StartColor = color;
            EndColor = color;
        }

        public void SetEndPoint(Vector2 delta)
        {
            this.delta = delta / rectTransform.lossyScale;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            int length = points.Length;

            vertexHelper.Clear();

            for (int i = 0; i < length; i++)
            {
                var point = points[i];
                var progress = (float)i / length;
                Color color = Color.Lerp(startColor, endColor, progress);

                Vector2 position = point * delta;
                Vector2 offset = point.Offset(delta) * thickness;
                vertexHelper.AddVert(position + offset, color, Vector3.zero);
                vertexHelper.AddVert(position - offset, color, Vector3.zero);
            }

            for (int i = 1; i < length; i++)
            {
                int startIdxLast = (i - 1) * 2;
                int startIdxHere = i * 2;

                vertexHelper.AddTriangle(startIdxLast, startIdxLast + 1, startIdxHere + 1);
                vertexHelper.AddTriangle(startIdxLast, startIdxHere + 1, startIdxHere);
            }
        }

        private Vector2 delta;
        private float thickness = 0.75f;
        private Color startColor = Color.blue;
        private Color endColor = Color.white;

        // Good enough for approximating the below function
        private readonly static FunctionPoint[] points = [0f, 1f/16f, 1f/8f, 1f/4f, 1f/2f, 3f/4f, 7f/8f, 15f/16f, 1f];

        // The Smoothstep function
        private static float Evaluate(float x) => x * x * (3.0f - x * 2.0f);

        // Should be kept up-to-date with the above function
        private static float EvaluateFirstDerivative(float x) => 6.0f * x * (1.0f - x);

        private readonly struct FunctionPoint
        {
            public Vector2 Offset(float x, float y)
            {
                if (Mathf.Abs(y) < OffsetVectorSwitchThreshold)
                {
                    return Vector2.up;
                }
                else if (Mathf.Abs(x) < OffsetVectorSwitchThreshold)
                {
                    return Vector2.left;
                }

                float ratio = x / y;
                float angle = Mathf.Atan(normal * ratio);

                return new(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            public Vector2 Offset(Vector2 delta) => Offset(delta.x, delta.y);

            public static implicit operator FunctionPoint(float x) => new(x);
            public static implicit operator Vector2(FunctionPoint point) => point.position;

            private FunctionPoint(float x)
            {
                position = new(x, Evaluate(x));
                normal = EvaluateNormal(x);
            }

            private readonly Vector2 position;
            private readonly float normal;

            private static float EvaluateNormal(float x) => -1.0f / EvaluateFirstDerivative(x);

            private const float OffsetVectorSwitchThreshold = 1f;
        }
    }
}
