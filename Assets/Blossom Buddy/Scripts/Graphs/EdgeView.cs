using UnityEngine;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Visual representation of an edge connecting two nodes
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class EdgeView : MonoBehaviour
    {
        [Header("Edge Settings")]
        [SerializeField] private float edgeWidth = 0.05f;
        [SerializeField] private Color edgeColor = Color.white;
        [SerializeField] private Material edgeMaterial;

        private LineRenderer lineRenderer;
        private Transform startNode;
        private Transform endNode;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            SetupLineRenderer();
        }

        /// <summary>
        /// Setup the line renderer with default settings
        /// </summary>
        private void SetupLineRenderer()
        {
            lineRenderer.startWidth = edgeWidth;
            lineRenderer.endWidth = edgeWidth;
            lineRenderer.startColor = edgeColor;
            lineRenderer.endColor = edgeColor;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;

            if (edgeMaterial != null)
            {
                lineRenderer.material = edgeMaterial;
            }
        }

        /// <summary>
        /// Initialize the edge with start and end nodes
        /// </summary>
        public void Initialize(Transform start, Transform end, string startId, string endId)
        {
            startNode = start;
            endNode = end;
            gameObject.name = $"Edge_{startId}_to_{endId}";
            UpdateEdge();
        }

        private void Update()
        {
            // Update edge position if nodes move
            UpdateEdge();
        }

        /// <summary>
        /// Update the edge line positions
        /// </summary>
        private void UpdateEdge()
        {
            if (startNode != null && endNode != null)
            {
                lineRenderer.SetPosition(0, startNode.position);
                lineRenderer.SetPosition(1, endNode.position);
            }
        }

        /// <summary>
        /// Set the edge color
        /// </summary>
        public void SetColor(Color color)
        {
            edgeColor = color;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        /// <summary>
        /// Set the edge width
        /// </summary>
        public void SetWidth(float width)
        {
            edgeWidth = width;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }
}

