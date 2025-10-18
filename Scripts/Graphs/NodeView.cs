using UnityEngine;
using TMPro;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Visual representation of a node in the graph
    /// </summary>
    public class NodeView : MonoBehaviour
    {
        [Header("Text References")]
        [SerializeField] private TextMeshPro idText;          // Text below the node
        [SerializeField] private TextMeshPro infoText;        // Text above the node
        
        [Header("Text Positioning")]
        [SerializeField] private float idOffsetY = -0.6f;     // Distance below node for ID
        [SerializeField] private float infoOffsetY = 1.0f;    // Distance above node for info
        [SerializeField] private float infoWidth = 3.5f;      // Width of info text box
        [SerializeField] private float infoHeight = 4.0f;     // Height of info text box
        
        [Header("Display Settings")]
        [SerializeField] private bool showInfo = true;
        [SerializeField] private bool showResources = true;
        
        private Node nodeData;
        private GameObject idTextObject;
        private GameObject infoTextObject;

        /// <summary>
        /// Initialize the node view with data
        /// </summary>
        public void Initialize(Node node)
        {
            nodeData = node;
            CreateTextElements();
            UpdateVisuals();
        }

        /// <summary>
        /// Create text elements for ID and info if they don't exist
        /// </summary>
        private void CreateTextElements()
        {
            // Create ID text below node if not assigned
            if (idText == null)
            {
                idTextObject = new GameObject("ID_Text");
                idTextObject.transform.SetParent(transform);
                idTextObject.transform.localPosition = new Vector3(0, idOffsetY, 0);
                idText = idTextObject.AddComponent<TextMeshPro>();
                idText.alignment = TextAlignmentOptions.Center;
                idText.fontSize = 0.5f;
                idText.color = Color.white;
                
                // Add Billboard component to make it always face the camera
                idTextObject.AddComponent<Billboard>();
            }

            // Create info text above node if not assigned
            if (infoText == null && showInfo)
            {
                infoTextObject = new GameObject("Info_Text");
                infoTextObject.transform.SetParent(transform);
                infoTextObject.transform.localPosition = new Vector3(0, infoOffsetY, 0);
                infoText = infoTextObject.AddComponent<TextMeshPro>();
                infoText.alignment = TextAlignmentOptions.TopLeft;
                infoText.fontSize = 0.8f;  // Increased from 0.3f
                infoText.color = Color.cyan;
                infoText.rectTransform.sizeDelta = new Vector2(infoWidth, infoHeight);
                infoText.enableWordWrapping = true;
                
                // Add Billboard component to make it always face the camera
                infoTextObject.AddComponent<Billboard>();
            }
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

            // Set the ID text below the node
            if (idText != null)
            {
                idText.text = nodeData.id;
            }

            // Set the info text above the node
            if (infoText != null && showInfo)
            {
                string infoContent = BuildInfoText();
                infoText.text = infoContent;
            }

            // Set the game object name for easier debugging
            gameObject.name = $"Node_{nodeData.id}";
        }

        /// <summary>
        /// Build the info text from node data
        /// </summary>
        private string BuildInfoText()
        {
            string info = "";

            // Add topic (larger and bold)
            if (!string.IsNullOrEmpty(nodeData.topic))
            {
                info += $"<size=120%><b>{nodeData.topic}</b></size>\n\n";
            }

            // Add content
            if (!string.IsNullOrEmpty(nodeData.small_content))
            {
                info += $"{nodeData.small_content}\n\n";
            }

            // Add summary
            if (!string.IsNullOrEmpty(nodeData.overall_summary))
            {
                info += $"<i>{nodeData.overall_summary}</i>\n";
            }

            // Add resources if enabled
            if (showResources && nodeData.resources != null && nodeData.resources.Count > 0)
            {
                info += "\n<size=80%><b>Resources:</b></size>\n";
                for (int i = 0; i < nodeData.resources.Count && i < 3; i++) // Show 3 resources now
                {
                    info += $"<size=70%>• {nodeData.resources[i]}</size>\n";
                }
                if (nodeData.resources.Count > 3)
                {
                    info += $"<size=70%>...and {nodeData.resources.Count - 3} more</size>\n";
                }
            }

            return info.Trim();
        }

        /// <summary>
        /// Get the node data
        /// </summary>
        public Node GetNodeData()
        {
            return nodeData;
        }

        /// <summary>
        /// Set whether to show info text
        /// </summary>
        public void SetShowInfo(bool show)
        {
            showInfo = show;
            if (infoText != null)
            {
                infoText.gameObject.SetActive(show);
            }
        }

        /// <summary>
        /// Set whether to show resources in info text
        /// </summary>
        public void SetShowResources(bool show)
        {
            showResources = show;
            if (nodeData != null)
            {
                UpdateVisuals();
            }
        }
    }
}