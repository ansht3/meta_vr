using UnityEngine;
using TMPro;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Visual representation of a node in the graph
    /// </summary>
    public class NodeView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshPro nodeText;
        
        private Node nodeData;

        /// <summary>
        /// Initialize the node view with data
        /// </summary>
        public void Initialize(Node node)
        {
            nodeData = node;
            UpdateVisuals();
        }

        /// <summary>
        /// Update the visual elements based on node data
        /// </summary>
        private void UpdateVisuals()
        {
            if (nodeData == null)
            {
                Debug.LogWarning("NodeView: No node data to display");
                return;
            }

            // Set the text to display the node ID
            if (nodeText != null)
            {
                nodeText.text = nodeData.id;
            }
            else
            {
                Debug.LogWarning($"NodeView: TextMeshPro component not assigned for node {nodeData.id}");
            }

            // Set the game object name for easier debugging
            gameObject.name = $"Node_{nodeData.id}";
        }

        /// <summary>
        /// Get the node data
        /// </summary>
        public Node GetNodeData()
        {
            return nodeData;
        }

        /// <summary>
        /// Handle node interaction (e.g., clicking)
        /// </summary>
        public void OnNodeClicked()
        {
            if (nodeData != null)
            {
                Debug.Log($"Node clicked: {nodeData.id} - {nodeData.topic}");
                // Add custom interaction logic here
            }
        }
    }
}

