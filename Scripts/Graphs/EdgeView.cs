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
            if (lineRenderer == null)
            {
                Debug.LogError("EdgeView: LineRenderer component not found!");
                return;
            }
            SetupLineRenderer();
        }

        /// <summary>
        /// Setup the line renderer with default settings
        /// </summary>
        private void SetupLineRenderer()
        {
            if (lineRenderer == null) return;

            lineRenderer.startWidth = edgeWidth;
            lineRenderer.endWidth = edgeWidth;
            lineRenderer.startColor = edgeColor;
            lineRenderer.endColor = edgeColor;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;

            // Create default material if none provided
            if (edgeMaterial != null)
            {
                lineRenderer.material = edgeMaterial;
                Debug.Log($"EdgeView: Using custom material for {gameObject.name}");
            }
            else
            {
                // Try multiple fallback shaders in order of preference
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Color");
                }
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Unlit");
                }
                if (shader == null)
                {
                    shader = Shader.Find("Hidden/InternalErrorShader");
                    Debug.LogWarning($"EdgeView: Could not find preferred shader, using fallback");
                }

                if (shader != null)
                {
                    lineRenderer.material = new Material(shader);
                    lineRenderer.material.color = edgeColor;
                    Debug.Log($"EdgeView: Created default material with shader '{shader.name}' for {gameObject.name}");
                }
                else
                {
                    Debug.LogError($"EdgeView: Could not find any shader! Edge will not be visible.");
                }
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

        /// <summary>
        /// Set the edge material
        /// </summary>
        public void SetMaterial(Material material)
        {
            edgeMaterial = material;
            if (lineRenderer != null && material != null)
            {
                lineRenderer.material = material;
            }
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
            if (lineRenderer == null) return;
            
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
            if (lineRenderer != null)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                
                // Also update material color if it exists
                if (lineRenderer.material != null)
                {
                    lineRenderer.material.color = color;
                }
            }
        }

        /// <summary>
        /// Set the edge width
        /// </summary>
        public void SetWidth(float width)
        {
            edgeWidth = width;
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;
            }
        }
    }
}