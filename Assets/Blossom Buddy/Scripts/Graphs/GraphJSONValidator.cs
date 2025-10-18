using UnityEngine;
using System.Collections.Generic;
// Requires "Newtonsoft Json" package (Window → Package Manager → search "Newtonsoft Json")
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Utility script to validate graph JSON stored directly in this component.
    /// Attach to any GameObject and use the Context Menu to run validation.
    /// </summary>
    public class GraphJSONValidator : MonoBehaviour
    {
        [Header("Validation Settings")]
        [Tooltip("The JSON object to validate (must contain an 'adjList' with 'subject', 'nodes', and optional 'edges').")]
        [TextArea(20, 80)]
        [SerializeField] private string jsonObject =
@"{
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

        [Header("Validation Results")]
        [SerializeField] private bool isValid = false;
        [SerializeField] private int nodeCount = 0;
        [SerializeField] private int edgeCount = 0;
        [SerializeField] private List<string> validationErrors = new List<string>();
        [SerializeField] private List<string> validationWarnings = new List<string>();

        /// <summary>
        /// Validate the JSON contained in jsonObject
        /// </summary>
        [ContextMenu("Validate JSON")]
        public void ValidateJSON()
        {
            ClearResults();

            Debug.Log("=== Validating in-memory JSON (jsonObject) ===");

            if (string.IsNullOrWhiteSpace(jsonObject))
            {
                AddError("jsonObject is empty.");
                LogResults();
                return;
            }

            try
            {
                var root = JToken.Parse(jsonObject);
                var adjList = root["adjList"] as JObject;

                if (adjList == null)
                {
                    AddError("Missing 'adjList' property or it is not an object.");
                    LogResults();
                    return;
                }

                ValidateAdjacencyList(adjList);

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
        private void ValidateAdjacencyList(JObject adjList)
        {
            // Subject
            string subject = adjList.Value<string>("subject");
            if (string.IsNullOrEmpty(subject))
            {
                AddWarning("Subject is empty");
            }
            else
            {
                Debug.Log($"Subject: {subject}");
            }

            // Nodes
            var nodes = adjList["nodes"] as JArray;
            if (nodes == null || nodes.Count == 0)
            {
                AddError("No nodes found in graph (adjList.nodes missing or empty).");
                return;
            }

            nodeCount = nodes.Count;
            Debug.Log($"Nodes: {nodeCount}");

            var nodeIds = new HashSet<string>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i] as JObject;
                ValidateNode(node, i, nodeIds);
            }

            // Edges
            var edges = adjList["edges"] as JArray;
            if (edges == null)
            {
                AddWarning("No edges array found");
            }
            else
            {
                edgeCount = edges.Count;
                Debug.Log($"Edges: {edgeCount}");

                for (int i = 0; i < edges.Count; i++)
                {
                    var edge = edges[i] as JArray;
                    ValidateEdge(edge, i, nodeIds);
                }
            }
        }

        /// <summary>
        /// Validate a single node object
        /// </summary>
        private void ValidateNode(JObject node, int index, HashSet<string> nodeIds)
        {
            if (node == null)
            {
                AddError($"Node at index {index} is not an object or is null");
                return;
            }

            string id = node.Value<string>("id");
            if (string.IsNullOrEmpty(id))
            {
                AddError($"Node at index {index} has no 'id'");
            }
            else
            {
                if (nodeIds.Contains(id))
                {
                    AddError($"Duplicate node ID: '{id}'");
                }
                else
                {
                    nodeIds.Add(id);
                }
            }

            string topic = node.Value<string>("topic");
            if (string.IsNullOrEmpty(topic))
            {
                AddWarning($"Node '{id}' has no 'topic'");
            }

            string smallContent = node.Value<string>("small_content");
            if (string.IsNullOrEmpty(smallContent))
            {
                AddWarning($"Node '{id}' has no 'small_content'");
            }

            string overallSummary = node.Value<string>("overall_summary");
            if (string.IsNullOrEmpty(overallSummary))
            {
                AddWarning($"Node '{id}' has no 'overall_summary'");
            }

            var resources = node["resources"] as JArray;
            if (resources == null)
            {
                AddWarning($"Node '{id}' has null 'resources' (use empty array [] instead)");
            }
            else if (resources.Count == 0)
            {
                AddWarning($"Node '{id}' has no resources");
            }
        }

        /// <summary>
        /// Validate a single edge (expected: [ ""startId"", ""endId"" ])
        /// </summary>
        private void ValidateEdge(JArray edge, int index, HashSet<string> validNodeIds)
        {
            if (edge == null)
            {
                AddError($"Edge at index {index} is not an array or is null");
                return;
            }

            if (edge.Count != 2)
            {
                AddError($"Edge at index {index} has {edge.Count} elements (expected 2)");
                return;
            }

            string startId = edge[0]?.ToString();
            string endId = edge[1]?.ToString();

            if (string.IsNullOrEmpty(startId))
            {
                AddError($"Edge {index} missing start node id (index 0)");
            }

            if (string.IsNullOrEmpty(endId))
            {
                AddError($"Edge {index} missing end node id (index 1)");
            }

            if (!string.IsNullOrEmpty(startId) && !validNodeIds.Contains(startId))
            {
                AddError($"Edge {index} references non-existent start node: '{startId}'");
            }

            if (!string.IsNullOrEmpty(endId) && !validNodeIds.Contains(endId))
            {
                AddError($"Edge {index} references non-existent end node: '{endId}'");
            }

            if (!string.IsNullOrEmpty(startId) && startId == endId)
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

        // Uncomment to auto-validate on scene start
        // private void Start() => ValidateJSON();
    }
}