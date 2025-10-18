using UnityEngine;
using TMPro;
using Oculus.Interaction;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Visual representation of a node in the graph
    /// </summary>
    public class NodeView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshPro nodeText;
        [SerializeField] private NodeInfoPanel infoPanel;
        
        [Header("Interaction")]
        [SerializeField] private bool showInfoPanelAlways = true;
        
        private Node nodeData;
        private NodeInfoPanel dedicatedInfoPanel;

        private void Start()
        {
            // Show info panel immediately if enabled
            if (showInfoPanelAlways && nodeData != null)
            {
                ShowPermanentInfoPanel();
            }
        }

        /// <summary>
        /// Initialize the node view with data
        /// </summary>
        public void Initialize(Node node)
        {
            nodeData = node;
            UpdateVisuals();
            
            // Show info panel immediately if enabled
            if (showInfoPanelAlways)
            {
                ShowPermanentInfoPanel();
            }
        }

        /// <summary>
        /// Create and show a permanent info panel for this node
        /// </summary>
        private void ShowPermanentInfoPanel()
        {
            if (nodeData == null) return;

            // Create dedicated info panel for this node
            if (dedicatedInfoPanel == null)
            {
                // Try to find the prefab-based panel first
                if (infoPanel != null)
                {
                    // Clone the shared panel prefab for this node
                    GameObject panelObject = Instantiate(infoPanel.gameObject);
                    panelObject.name = $"InfoPanel_{nodeData.id}";
                    dedicatedInfoPanel = panelObject.GetComponent<NodeInfoPanel>();
                }
                else
                {
                    Debug.LogWarning($"NodeView: No info panel prefab found for {nodeData.id}");
                    return;
                }
            }

            // Show the panel with this node's data
            dedicatedInfoPanel.ShowNodeInfo(nodeData, transform);
            
            Debug.Log($"NodeView: Displaying permanent info panel for {nodeData.id}");
        }

        /// <summary>
        /// Set the info panel reference
        /// </summary>
        public void SetInfoPanel(NodeInfoPanel panel)
        {
            infoPanel = panel;
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

    }
}