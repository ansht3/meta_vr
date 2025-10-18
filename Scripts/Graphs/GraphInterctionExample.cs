using UnityEngine;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Example script demonstrating how to interact with the graph system
    /// Attach this to any GameObject to test graph interactions
    /// </summary>
    public class GraphInteractionExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GraphManager graphManager;

        [Header("Test Settings")]
        [SerializeField] private string testNodeId = "eq_start";
        [SerializeField] private Color highlightColor = Color.yellow;

        private void Start()
        {
            // Find GraphManager if not assigned
            if (graphManager == null)
            {
                graphManager = FindFirstObjectByType<GraphManager>();
            }

            if (graphManager == null)
            {
                Debug.LogError("GraphInteractionExample: No GraphManager found in scene");
                return;
            }

            // Wait a frame for the graph to be initialized
            Invoke(nameof(DemonstrateGraphInteractions), 0.5f);
        }

        /// <summary>
        /// Demonstrates various ways to interact with the graph
        /// </summary>
        private void DemonstrateGraphInteractions()
        {
            Debug.Log("=== Graph Interaction Examples ===");

            // Example 1: Get graph data
            GraphData data = graphManager.GetGraphData();
            if (data != null && data.adjList != null)
            {
                Debug.Log($"Graph Subject: {data.adjList.subject}");
                Debug.Log($"Total Nodes: {data.adjList.nodes.Count}");
                Debug.Log($"Total Edges: {data.adjList.edges.Count}");
            }

            // Example 2: Get a specific node
            NodeView node = graphManager.GetNodeView(testNodeId);
            if (node != null)
            {
                Node nodeData = node.GetNodeData();
                Debug.Log($"\nNode '{testNodeId}' found:");
                Debug.Log($"  Topic: {nodeData.topic}");
                Debug.Log($"  Content: {nodeData.small_content}");
                Debug.Log($"  Summary: {nodeData.overall_summary}");
                Debug.Log($"  Resources: {nodeData.resources.Count}");

                // Highlight this node
                HighlightNode(node.gameObject);
            }
            else
            {
                Debug.LogWarning($"Node '{testNodeId}' not found!");
            }

            // Example 3: Iterate through all nodes
            var allNodes = graphManager.GetAllNodeViews();
            Debug.Log($"\n=== All Nodes ({allNodes.Count}) ===");
            foreach (var kvp in allNodes)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value.GetNodeData().topic}");
            }

            // Example 4: Find nodes by topic keyword
            string searchKeyword = "equation";
            Debug.Log($"\n=== Nodes containing '{searchKeyword}' ===");
            foreach (var kvp in allNodes)
            {
                Node nodeData = kvp.Value.GetNodeData();
                if (nodeData.topic.ToLower().Contains(searchKeyword.ToLower()))
                {
                    Debug.Log($"  Found: {kvp.Key} - {nodeData.topic}");
                }
            }
        }

        /// <summary>
        /// Highlight a node by changing its color
        /// </summary>
        private void HighlightNode(GameObject nodeObject)
        {
            Renderer renderer = nodeObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                // Store original material
                Material originalMaterial = renderer.material;
                
                // Create a new material with highlight color
                Material highlightMaterial = new Material(originalMaterial);
                highlightMaterial.color = highlightColor;
                renderer.material = highlightMaterial;

                Debug.Log($"Highlighted node: {nodeObject.name}");
            }
        }

        /// <summary>
        /// Example: Reload graph with a new JSON file
        /// Call this from a button or other trigger
        /// </summary>
        public void LoadDifferentGraph(string jsonFileName)
        {
            if (graphManager != null)
            {
                Debug.Log($"Loading new graph: {jsonFileName}");
                // Note: You would need to make jsonFileName public in GraphManager
                // or add a LoadGraph(string fileName) method
                graphManager.ReloadGraph();
            }
        }

        /// <summary>
        /// Example: Get all connected nodes from a starting node
        /// </summary>
        public void FindConnectedNodes(string startNodeId)
        {
            GraphData data = graphManager.GetGraphData();
            if (data == null || data.adjList == null) return;

            Debug.Log($"=== Nodes connected to '{startNodeId}' ===");
            
            foreach (var edge in data.adjList.edges)
            {
                if (edge.Count < 2) continue;

                // If this edge starts from our node
                if (edge[0] == startNodeId)
                {
                    NodeView connectedNode = graphManager.GetNodeView(edge[1]);
                    if (connectedNode != null)
                    {
                        Debug.Log($"  → {edge[1]}: {connectedNode.GetNodeData().topic}");
                    }
                }
                // If this edge ends at our node (incoming connection)
                else if (edge[1] == startNodeId)
                {
                    NodeView connectedNode = graphManager.GetNodeView(edge[0]);
                    if (connectedNode != null)
                    {
                        Debug.Log($"  ← {edge[0]}: {connectedNode.GetNodeData().topic}");
                    }
                }
            }
        }

        // Keyboard shortcuts for testing
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Reloading graph...");
                graphManager.ReloadGraph();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                DemonstrateGraphInteractions();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                FindConnectedNodes(testNodeId);
            }
        }
    }
}