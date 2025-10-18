using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Utility script to validate graph JSON files
    /// Attach to any GameObject and call ValidateJSON() in the Inspector or via code
    /// </summary>
    public class GraphJSONValidator : MonoBehaviour
    {
        [Header("Validation Settings")]
        [SerializeField] private string jsonFileName = "example.json";
        [Tooltip("JSON file should be located in Assets/Blossom Buddy/Scripts/graph_data/")]

        [Header("Validation Results")]
        [SerializeField] private bool isValid = false;
        [SerializeField] private int nodeCount = 0;
        [SerializeField] private int edgeCount = 0;
        [SerializeField] private List<string> validationErrors = new List<string>();
        [SerializeField] private List<string> validationWarnings = new List<string>();

        /// <summary>
        /// Validate the JSON file
        /// </summary>
        [ContextMenu("Validate JSON")]
        public void ValidateJSON()
        {
            ClearResults();
            
            string filePath = Path.Combine(Application.dataPath, "Blossom Buddy", "Scripts", "graph_data", jsonFileName);

            Debug.Log($"=== Validating JSON: {jsonFileName} ===");

            // Check if file exists
            if (!File.Exists(filePath))
            {
                AddError($"File not found at: {filePath}");
                LogResults();
                return;
            }

            try
            {
                // Read and parse JSON
                string jsonContent = File.ReadAllText(filePath);
                GraphData graphData = JsonUtility.FromJson<GraphData>(jsonContent);

                if (graphData == null)
                {
                    AddError("Failed to parse JSON - null result");
                    LogResults();
                    return;
                }

                if (graphData.adjList == null)
                {
                    AddError("Missing 'adjList' property");
                    LogResults();
                    return;
                }

                // Validate structure
                ValidateAdjacencyList(graphData.adjList);

                // If no errors, mark as valid
                if (validationErrors.Count == 0)
                {
                    isValid = true;
                    Debug.Log("<color=green>✓ JSON is valid!</color>");
                }
                else
                {
                    isValid = false;
                    Debug.LogError("<color=red>✗ JSON validation failed</color>");
                }
            }
            catch (System.Exception e)
            {
                AddError($"Exception during parsing: {e.Message}");
            }

            LogResults();
        }

        /// <summary>
        /// Validate the adjacency list structure
        /// </summary>
        private void ValidateAdjacencyList(AdjacencyList adjList)
        {
            // Check subject
            if (string.IsNullOrEmpty(adjList.subject))
            {
                AddWarning("Subject is empty");
            }
            else
            {
                Debug.Log($"Subject: {adjList.subject}");
            }

            // Validate nodes
            if (adjList.nodes == null || adjList.nodes.Count == 0)
            {
                AddError("No nodes found in graph");
                return;
            }

            nodeCount = adjList.nodes.Count;
            Debug.Log($"Nodes: {nodeCount}");

            HashSet<string> nodeIds = new HashSet<string>();
            for (int i = 0; i < adjList.nodes.Count; i++)
            {
                ValidateNode(adjList.nodes[i], i, nodeIds);
            }

            // Validate edges
            if (adjList.edges == null)
            {
                AddWarning("No edges array found");
            }
            else
            {
                edgeCount = adjList.edges.Count;
                Debug.Log($"Edges: {edgeCount}");

                for (int i = 0; i < adjList.edges.Count; i++)
                {
                    ValidateEdge(adjList.edges[i], i, nodeIds);
                }
            }
        }

        /// <summary>
        /// Validate a single node
        /// </summary>
        private void ValidateNode(Node node, int index, HashSet<string> nodeIds)
        {
            if (node == null)
            {
                AddError($"Node at index {index} is null");
                return;
            }

            // Check ID
            if (string.IsNullOrEmpty(node.id))
            {
                AddError($"Node at index {index} has no ID");
            }
            else
            {
                // Check for duplicate IDs
                if (nodeIds.Contains(node.id))
                {
                    AddError($"Duplicate node ID: '{node.id}'");
                }
                else
                {
                    nodeIds.Add(node.id);
                }
            }

            // Check topic
            if (string.IsNullOrEmpty(node.topic))
            {
                AddWarning($"Node '{node.id}' has no topic");
            }

            // Check content
            if (string.IsNullOrEmpty(node.small_content))
            {
                AddWarning($"Node '{node.id}' has no small_content");
            }

            // Check summary
            if (string.IsNullOrEmpty(node.overall_summary))
            {
                AddWarning($"Node '{node.id}' has no overall_summary");
            }

            // Check resources
            if (node.resources == null)
            {
                AddWarning($"Node '{node.id}' has null resources array (use empty array [] instead)");
            }
            else if (node.resources.Count == 0)
            {
                AddWarning($"Node '{node.id}' has no resources");
            }
        }

        /// <summary>
        /// Validate a single edge
        /// </summary>
        private void ValidateEdge(List<string> edge, int index, HashSet<string> validNodeIds)
        {
            if (edge == null)
            {
                AddError($"Edge at index {index} is null");
                return;
            }

            if (edge.Count != 2)
            {
                AddError($"Edge at index {index} has {edge.Count} elements (expected 2)");
                return;
            }

            string startId = edge[0];
            string endId = edge[1];

            // Check if nodes exist
            if (!validNodeIds.Contains(startId))
            {
                AddError($"Edge {index} references non-existent start node: '{startId}'");
            }

            if (!validNodeIds.Contains(endId))
            {
                AddError($"Edge {index} references non-existent end node: '{endId}'");
            }

            // Check for self-loops
            if (startId == endId)
            {
                AddWarning($"Edge {index} is a self-loop: '{startId}' → '{endId}'");
            }
        }

        /// <summary>
        /// Add an error to the list
        /// </summary>
        private void AddError(string error)
        {
            validationErrors.Add(error);
            Debug.LogError($"ERROR: {error}");
        }

        /// <summary>
        /// Add a warning to the list
        /// </summary>
        private void AddWarning(string warning)
        {
            validationWarnings.Add(warning);
            Debug.LogWarning($"WARNING: {warning}");
        }

        /// <summary>
        /// Clear all validation results
        /// </summary>
        private void ClearResults()
        {
            isValid = false;
            nodeCount = 0;
            edgeCount = 0;
            validationErrors.Clear();
            validationWarnings.Clear();
        }

        /// <summary>
        /// Log validation results
        /// </summary>
        private void LogResults()
        {
            Debug.Log("=== Validation Summary ===");
            Debug.Log($"Valid: {isValid}");
            Debug.Log($"Errors: {validationErrors.Count}");
            Debug.Log($"Warnings: {validationWarnings.Count}");

            if (validationErrors.Count > 0)
            {
                Debug.Log("\n--- Errors ---");
                foreach (var error in validationErrors)
                {
                    Debug.LogError($"  • {error}");
                }
            }

            if (validationWarnings.Count > 0)
            {
                Debug.Log("\n--- Warnings ---");
                foreach (var warning in validationWarnings)
                {
                    Debug.LogWarning($"  • {warning}");
                }
            }

            Debug.Log("====================");
        }

        /// <summary>
        /// Validate on start if desired
        /// </summary>
        private void Start()
        {
            // Uncomment to auto-validate on scene start
            // ValidateJSON();
        }
    }
}

