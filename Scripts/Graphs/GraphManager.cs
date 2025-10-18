using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Manages the loading and visualization of graph data from JSON files
    /// </summary>
    public class GraphManager : MonoBehaviour
    {
        [Header("Prefab Settings")]
        [SerializeField] private GameObject nodePrefab;
        [Tooltip("Prefab should contain a sphere and a TextMeshPro component for displaying the node ID")]

        [Header("Edge Settings")]
        [SerializeField] private bool showEdges = true;
        [SerializeField] private GameObject edgePrefab;
        [SerializeField] private Material edgeMaterial;
        [SerializeField] private Color edgeColor = Color.cyan;
        [SerializeField] private float edgeWidth = 0.05f;

        [Header("JSON Settings")]
        [SerializeField] private string jsonFileName = "example.json";
        [Tooltip("JSON file should be located in Assets/StreamingAssets/graph_data/")]

        [Header("Layout Settings")]
        [SerializeField] private float nodeSpacing = 2f;
        [SerializeField] private float circleRadius = 5f;
        [SerializeField] private bool useCircularLayout = true;

        [Header("Parent Transform")]
        [SerializeField] private Transform graphParent;
        [SerializeField] private Transform edgesParent;

        private GraphData graphData;
        private Dictionary<string, NodeView> nodeViews = new Dictionary<string, NodeView>();
        private Dictionary<string, GameObject> nodeObjects = new Dictionary<string, GameObject>();
        private List<EdgeView> edgeViews = new List<EdgeView>();
        private List<GameObject> edgeObjects = new List<GameObject>();

        private void Start()
        {
            if (graphParent == null)
            {
                // Create a parent object for organizing nodes
                GameObject parent = new GameObject("GraphNodes");
                graphParent = parent.transform;
            }

            if (edgesParent == null)
            {
                // Create a parent object for organizing edges
                GameObject parent = new GameObject("GraphEdges");
                edgesParent = parent.transform;
            }

            // Don't load graph automatically - wait for external trigger
            Debug.Log("GraphManager: Ready. No graph loaded at start. Waiting for LoadGraphFromData() call.");
        }

        /// <summary>
        /// Load graph data from JSON file (Coroutine for cross-platform support)
        /// </summary>
        private IEnumerator LoadGraphFromJSONCoroutine()
        {
            // Try persistentDataPath first (writable, for dynamically generated graphs)
            string persistentFilePath = Path.Combine(Application.persistentDataPath, "graph_data", jsonFileName);
            string streamingFilePath = Path.Combine(Application.streamingAssetsPath, "graph_data", jsonFileName);
            
            string filePath = persistentFilePath;
            bool usePersistent = File.Exists(persistentFilePath);
            
            if (!usePersistent)
            {
                filePath = streamingFilePath;
                Debug.Log($"GraphManager: No file in persistentDataPath, using StreamingAssets");
            }
            else
            {
                Debug.Log($"GraphManager: Found file in persistentDataPath (dynamically generated)");
            }
            
            Debug.Log($"GraphManager: Loading JSON from {filePath}");

            string jsonContent = null;

            // On Android, StreamingAssets requires UnityWebRequest (but persistentDataPath can use File.ReadAllText)
            if (Application.platform == RuntimePlatform.Android && !usePersistent)
            {
                UnityWebRequest request = UnityWebRequest.Get(filePath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    jsonContent = request.downloadHandler.text;
                }
                else
                {
                    Debug.LogError($"GraphManager: Failed to load JSON from {filePath}");
                    Debug.LogError($"Error: {request.error}");
                    yield break;
                }
            }
            else
            {
                // On other platforms, or when using persistentDataPath, use File.ReadAllText
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"GraphManager: JSON file not found at {filePath}");
                    yield break;
                }

                try
                {
                    jsonContent = File.ReadAllText(filePath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"GraphManager: Error reading file: {e.Message}");
                    yield break;
                }
            }

            // Parse JSON
            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError("GraphManager: JSON content is empty");
                yield break;
            }

            try
            {
                graphData = JsonUtility.FromJson<GraphData>(jsonContent);

                if (graphData == null || graphData.adjList == null)
                {
                    Debug.LogError("GraphManager: Failed to parse JSON data");
                    yield break;
                }

                Debug.Log($"GraphManager: Successfully loaded graph '{graphData.adjList.subject}' with {graphData.adjList.nodes.Count} nodes");

                // Create visuals after loading
                CreateNodeVisuals();
                
                if (showEdges)
                {
                    CreateEdgeVisuals();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GraphManager: Error parsing JSON: {e.Message}");
            }
        }

        /// <summary>
        /// Create visual representations for all nodes
        /// </summary>
        private void CreateNodeVisuals()
        {
            if (graphData == null || graphData.adjList == null || graphData.adjList.nodes == null)
            {
                Debug.LogError("GraphManager: No graph data available to visualize");
                return;
            }

            if (nodePrefab == null)
            {
                Debug.LogError("GraphManager: Node prefab is not assigned!");
                return;
            }

            int nodeCount = graphData.adjList.nodes.Count;
            
            for (int i = 0; i < nodeCount; i++)
            {
                Node node = graphData.adjList.nodes[i];
                Vector3 position = CalculateNodePosition(i, nodeCount);

                // Instantiate the node prefab
                GameObject nodeObject = Instantiate(nodePrefab, position, Quaternion.identity, graphParent);
                
                // Get or add NodeView component
                NodeView nodeView = nodeObject.GetComponent<NodeView>();
                if (nodeView == null)
                {
                    nodeView = nodeObject.AddComponent<NodeView>();
                }

                // Initialize the node with data (this creates the text elements)
                nodeView.Initialize(node);

                // Store references
                nodeViews[node.id] = nodeView;
                nodeObjects[node.id] = nodeObject;
            }

            Debug.Log($"GraphManager: Created {nodeCount} node visuals");
        }

        /// <summary>
        /// Create visual representations for all edges
        /// </summary>
        private void CreateEdgeVisuals()
        {
            if (graphData == null || graphData.adjList == null || graphData.adjList.edges == null)
            {
                Debug.LogWarning("GraphManager: No edge data available to visualize");
                return;
            }

            Debug.Log($"GraphManager: Starting to create {graphData.adjList.edges.Count} edges...");

            foreach (var edge in graphData.adjList.edges)
            {
                if (edge.Count < 2)
                {
                    Debug.LogWarning("GraphManager: Invalid edge format - expected [startId, endId]");
                    continue;
                }

                string startId = edge[0];
                string endId = edge[1];

                if (!nodeObjects.ContainsKey(startId) || !nodeObjects.ContainsKey(endId))
                {
                    Debug.LogWarning($"GraphManager: Cannot create edge - node not found (start: {startId}, end: {endId})");
                    continue;
                }

                GameObject startNode = nodeObjects[startId];
                GameObject endNode = nodeObjects[endId];

                // Create edge object
                GameObject edgeObject;
                if (edgePrefab != null)
                {
                    edgeObject = Instantiate(edgePrefab, edgesParent);
                }
                else
                {
                    // Create a default edge object with LineRenderer
                    edgeObject = new GameObject($"Edge_{startId}_to_{endId}");
                    edgeObject.transform.SetParent(edgesParent);
                    edgeObject.AddComponent<LineRenderer>();
                }

                // Get or add EdgeView component
                EdgeView edgeView = edgeObject.GetComponent<EdgeView>();
                if (edgeView == null)
                {
                    edgeView = edgeObject.AddComponent<EdgeView>();
                }

                // Initialize the edge
                edgeView.Initialize(startNode.transform, endNode.transform, startId, endId);
                edgeView.SetColor(edgeColor);
                edgeView.SetWidth(edgeWidth);
                
                // Set material if provided
                if (edgeMaterial != null)
                {
                    edgeView.SetMaterial(edgeMaterial);
                    Debug.Log($"GraphManager: Using custom edge material for {startId}->{endId}");
                }
                else
                {
                    Debug.Log($"GraphManager: No custom material, using default for {startId}->{endId}");
                }

                edgeViews.Add(edgeView);
                edgeObjects.Add(edgeObject);
            }

            Debug.Log($"<color=green>GraphManager: Successfully created {edgeObjects.Count} edge visuals</color>");
            Debug.Log($"Edge settings - Width: {edgeWidth}, Color: {edgeColor}, ShowEdges: {showEdges}");
        }

        /// <summary>
        /// Calculate position for a node based on layout settings
        /// </summary>
        private Vector3 CalculateNodePosition(int index, int totalNodes)
        {
            if (useCircularLayout)
            {
                // Circular layout
                float angle = (index / (float)totalNodes) * 2f * Mathf.PI;
                float x = Mathf.Cos(angle) * circleRadius;
                float z = Mathf.Sin(angle) * circleRadius;
                return new Vector3(x, 0, z);
            }
            else
            {
                // Grid layout
                int columns = Mathf.CeilToInt(Mathf.Sqrt(totalNodes));
                int row = index / columns;
                int col = index % columns;
                return new Vector3(col * nodeSpacing, 0, row * nodeSpacing);
            }
        }

        /// <summary>
        /// Get a node view by its ID
        /// </summary>
        public NodeView GetNodeView(string nodeId)
        {
            if (nodeViews.ContainsKey(nodeId))
            {
                return nodeViews[nodeId];
            }
            return null;
        }

        /// <summary>
        /// Get all node views
        /// </summary>
        public Dictionary<string, NodeView> GetAllNodeViews()
        {
            return nodeViews;
        }

        /// <summary>
        /// Get the graph data
        /// </summary>
        public GraphData GetGraphData()
        {
            return graphData;
        }

        /// <summary>
        /// Load graph from a GraphData object (no file needed)
        /// </summary>
        public void LoadGraphFromData(GraphData data)
        {
            if (data == null || data.adjList == null)
            {
                Debug.LogError("GraphManager: Cannot load null graph data");
                return;
            }

            Debug.Log($"GraphManager: Loading graph from data object - Subject: {data.adjList.subject}");
            
            // Clear existing graph
            ClearGraph();
            
            // Set the data
            graphData = data;
            
            Debug.Log($"GraphManager: Successfully loaded graph '{graphData.adjList.subject}' with {graphData.adjList.nodes.Count} nodes");

            // Create visuals
            CreateNodeVisuals();
            
            if (showEdges)
            {
                CreateEdgeVisuals();
            }
        }

        /// <summary>
        /// Clear the current graph
        /// </summary>
        private void ClearGraph()
        {
            // Clear existing nodes
            foreach (var nodeObject in nodeObjects.Values)
            {
                Destroy(nodeObject);
            }
            nodeViews.Clear();
            nodeObjects.Clear();

            // Clear existing edges
            foreach (var edgeObject in edgeObjects)
            {
                Destroy(edgeObject);
            }
            edgeViews.Clear();
            edgeObjects.Clear();
        }

        /// <summary>
        /// Reload the graph from JSON
        /// </summary>
        public void ReloadGraph()
        {
            // Clear existing nodes
            foreach (var nodeObject in nodeObjects.Values)
            {
                Destroy(nodeObject);
            }
            nodeViews.Clear();
            nodeObjects.Clear();

            // Clear existing edges
            foreach (var edgeObject in edgeObjects)
            {
                Destroy(edgeObject);
            }
            edgeViews.Clear();
            edgeObjects.Clear();

            // Reload
            StartCoroutine(LoadGraphFromJSONCoroutine());
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Reload graph in edit mode
        /// </summary>
        [ContextMenu("Reload Graph")]
        private void ReloadGraphInEditor()
        {
            if (Application.isPlaying)
            {
                ReloadGraph();
            }
            else
            {
                Debug.LogWarning("GraphManager: Reload is only available in Play mode");
            }
        }
#endif
    }
}