using UnityEngine;
using System.Collections.Generic;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Manages the loading and visualization of graph data from embedded JSON
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

        // Embedded JSON data
        private const string GRAPH_JSON = @"{
  ""adjList"": {
    ""subject"": ""Linear equations — equivalence and solving steps"",
    ""nodes"": [
      {
        ""id"": ""eq_start"",
        ""topic"": ""Original equation"",
        ""small_content"": ""Given: 3(x+1) − x = 9"",
        ""resources"": [
          ""https://www.khanacademy.org/math/algebra/x2f8bb11595b61c86:foundation-algebra/x2f8bb11595b61c86:distributive-property/v/the-distributive-property"",
          ""https://www.mathsisfun.com/algebra/like-terms.html""
        ],
        ""overall_summary"": ""Start with a linear equation that will be transformed via equivalence-preserving steps.""
      },
      {
        ""id"": ""distribute"",
        ""topic"": ""Distribute multiplication over addition"",
        ""small_content"": ""3(x+1) → 3x + 3, so 3x + 3 − x = 9"",
        ""resources"": [
          ""https://www.khanacademy.org/math/algebra/x2f8bb11595b61c86:foundation-algebra/x2f8bb11595b61c86:distributive-property/a/distributive-property-explained""
        ],
        ""overall_summary"": ""Apply the distributive property to remove parentheses.""
      },
      {
        ""id"": ""combine"",
        ""topic"": ""Combine like terms"",
        ""small_content"": ""3x − x = 2x, so 2x + 3 = 9"",
        ""resources"": [
          ""https://www.khanacademy.org/math/algebra/x2f8bb11595b61c86:foundation-algebra/x2f8bb11595b61c86:combining-like-terms/v/combining-like-terms""
        ],
        ""overall_summary"": ""Group x-terms to simplify the expression.""
      },
      {
        ""id"": ""isolate_coeff"",
        ""topic"": ""Isolate variable term by addition/subtraction"",
        ""small_content"": ""Subtract 3 from both sides → 2x = 6"",
        ""resources"": [
          ""https://www.khanacademy.org/math/algebra/x2f8bb11595b61c86:solve-equations-inequalities/x2f8bb11595b61c86:linear-equations-one-variable/v/why-we-do-same-thing-to-both-sides-one-step-equations""
        ],
        ""overall_summary"": ""Use an equivalence operation to move constants.""
      },
      {
        ""id"": ""solve"",
        ""topic"": ""Solve for x by division"",
        ""small_content"": ""Divide both sides by 2 → x = 3"",
        ""resources"": [
          ""https://www.khanacademy.org/math/algebra/x2f8bb11595b61c86:solve-equations-inequalities/x2f8bb11595b61c86:linear-equations-one-variable/v/solving-basic-equation-with-variable-on-one-side""
        ],
        ""overall_summary"": ""Finish isolation of x; division by a nonzero number preserves equivalence.""
      },
      {
        ""id"": ""check"",
        ""topic"": ""Check the solution"",
        ""small_content"": ""Plug x=3 into 3(x+1)−x: 3(4)−3=12−3=9 ✓"",
        ""resources"": [
          ""https://www.mathsisfun.com/algebra/substitution.html""
        ],
        ""overall_summary"": ""Verification shows the transformed steps stayed equivalent to the original.""
      },
      {
        ""id"": ""equiv_rules"",
        ""topic"": ""Equivalence rules"",
        ""small_content"": ""Add/subtract same value; multiply/divide by a NONZERO constant; distribute/collect like terms."",
        ""resources"": [
          ""https://www.khanacademy.org/math/algebra/x2f8bb11595b61c86:foundation-algebra/x2f8bb11595b61c86:algebraic-properties/v/introduction-to-properties-of-addition""
        ],
        ""overall_summary"": ""These operations keep the solution set unchanged.""
      },
      {
        ""id"": ""right_panel_eq"",
        ""topic"": ""Illustrative side example"",
        ""small_content"": ""5x = 6x. Subtract 5x → 0 = x."",
        ""resources"": [
          ""https://www.khanacademy.org/math/algebra/x2f8bb11595b61c86:solve-equations-inequalities/x2f8bb11595b61c86:linear-equations-one-variable/v/solving-equations-variables-both-sides""
        ],
        ""overall_summary"": ""Legitimate step shows that the only value satisfying 5x=6x is x=0.""
      },
      {
        ""id"": ""caution"",
        ""topic"": ""Caution about invalid operations"",
        ""small_content"": ""Dividing by a variable that could be 0 can break equivalence (e.g., dividing both sides by x when x might be 0)."",
        ""resources"": [
          ""https://www.mathsisfun.com/dividing-by-zero.html""
        ],
        ""overall_summary"": ""Some manipulations create extraneous/losing solutions; avoid non-permitted operations.""
      },
      {
        ""id"": ""x_equals_2_note"",
        ""topic"": ""Non-solution note"",
        ""small_content"": ""x = 2 (shown in image header) does NOT satisfy 3(x+1)−x=9."",
        ""resources"": [
          ""https://www.mathsisfun.com/algebra/substitution.html""
        ],
        ""overall_summary"": ""Serves as a contrast to the correct solution x=3.""
      }
    ],
    ""edges"": [
      [""eq_start"", ""distribute""],
      [""distribute"", ""combine""],
      [""combine"", ""isolate_coeff""],
      [""isolate_coeff"", ""solve""],
      [""solve"", ""check""],
      [""equiv_rules"", ""distribute""],
      [""equiv_rules"", ""combine""],
      [""equiv_rules"", ""isolate_coeff""],
      [""equiv_rules"", ""solve""],
      [""right_panel_eq"", ""caution""],
      [""caution"", ""equiv_rules""],
      [""x_equals_2_note"", ""eq_start""]
    ]
  }
}";

        private void Start()
        {
            if (graphParent == null)
            {
                GameObject parent = new GameObject("GraphNodes");
                graphParent = parent.transform;
            }

            if (edgesParent == null)
            {
                GameObject parent = new GameObject("GraphEdges");
                edgesParent = parent.transform;
            }

            LoadGraphFromJSON();
            CreateNodeVisuals();
            
            if (showEdges)
            {
                CreateEdgeVisuals();
            }
        }

        /// <summary>
        /// Load graph data from embedded JSON string
        /// </summary>
        private void LoadGraphFromJSON()
        {
            try
            {
                graphData = JsonUtility.FromJson<GraphData>(GRAPH_JSON);

                if (graphData == null || graphData.adjList == null)
                {
                    Debug.LogError("GraphManager: Failed to parse JSON data");
                    return;
                }

                Debug.Log($"GraphManager: Successfully loaded graph '{graphData.adjList.subject}' with {graphData.adjList.nodes.Count} nodes");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GraphManager: Error loading JSON: {e.Message}");
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

                GameObject nodeObject = Instantiate(nodePrefab, position, Quaternion.identity, graphParent);
                
                NodeView nodeView = nodeObject.GetComponent<NodeView>();
                if (nodeView == null)
                {
                    nodeView = nodeObject.AddComponent<NodeView>();
                }

                nodeView.Initialize(node);

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

                GameObject edgeObject;
                if (edgePrefab != null)
                {
                    edgeObject = Instantiate(edgePrefab, edgesParent);
                }
                else
                {
                    edgeObject = new GameObject($"Edge_{startId}_to_{endId}");
                    edgeObject.transform.SetParent(edgesParent);
                    edgeObject.AddComponent<LineRenderer>();
                }

                EdgeView edgeView = edgeObject.GetComponent<EdgeView>();
                if (edgeView == null)
                {
                    edgeView = edgeObject.AddComponent<EdgeView>();
                }

                edgeView.Initialize(startNode.transform, endNode.transform, startId, endId);
                edgeView.SetColor(edgeColor);
                edgeView.SetWidth(edgeWidth);

                edgeViews.Add(edgeView);
                edgeObjects.Add(edgeObject);
            }

            Debug.Log($"GraphManager: Created {edgeObjects.Count} edge visuals");
        }

        /// <summary>
        /// Calculate position for a node based on layout settings
        /// </summary>
        private Vector3 CalculateNodePosition(int index, int totalNodes)
        {
            if (useCircularLayout)
            {
                float angle = (index / (float)totalNodes) * 2f * Mathf.PI;
                float x = Mathf.Cos(angle) * circleRadius;
                float z = Mathf.Sin(angle) * circleRadius;
                return new Vector3(x, 0, z);
            }
            else
            {
                int columns = Mathf.CeilToInt(Mathf.Sqrt(totalNodes));
                int row = index / columns;
                int col = index % columns;
                return new Vector3(col * nodeSpacing, 0, row * nodeSpacing);
            }
        }

        public NodeView GetNodeView(string nodeId)
        {
            if (nodeViews.ContainsKey(nodeId))
            {
                return nodeViews[nodeId];
            }
            return null;
        }

        public Dictionary<string, NodeView> GetAllNodeViews()
        {
            return nodeViews;
        }

        public GraphData GetGraphData()
        {
            return graphData;
        }

        public void ReloadGraph()
        {
            foreach (var nodeObject in nodeObjects.Values)
            {
                Destroy(nodeObject);
            }
            nodeViews.Clear();
            nodeObjects.Clear();

            foreach (var edgeObject in edgeObjects)
            {
                Destroy(edgeObject);
            }
            edgeViews.Clear();
            edgeObjects.Clear();

            LoadGraphFromJSON();
            CreateNodeVisuals();
            
            if (showEdges)
            {
                CreateEdgeVisuals();
            }
        }

#if UNITY_EDITOR
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