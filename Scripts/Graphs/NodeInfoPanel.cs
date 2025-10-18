using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Displays detailed information about a node in a floating UI panel
    /// </summary>
    public class NodeInfoPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI topicText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI summaryText;
        [SerializeField] private TextMeshProUGUI resourcesText;
        [SerializeField] private GameObject panelBackground;

        [Header("Panel Settings")]
        [SerializeField] private float offsetDistance = 1.5f;
        [SerializeField] private Vector3 offsetDirection = Vector3.right;
        [SerializeField] private bool faceCamera = true;

        private Transform targetNode;
        private Camera mainCamera;
        private bool isVisible = false;

        private void Start()
        {
            mainCamera = Camera.main;
            Hide();
        }

        /// <summary>
        /// Show the panel with node information
        /// </summary>
        public void ShowNodeInfo(Node node, Transform nodeTransform)
        {
            if (node == null)
            {
                Debug.LogWarning("NodeInfoPanel: Cannot show null node");
                return;
            }

            targetNode = nodeTransform;

            // Update UI text
            if (topicText != null)
                topicText.text = node.topic;

            if (contentText != null)
                contentText.text = node.small_content;

            if (summaryText != null)
                summaryText.text = node.overall_summary;

            if (resourcesText != null)
            {
                if (node.resources != null && node.resources.Count > 0)
                {
                    string resourceList = "Resources:\n";
                    for (int i = 0; i < node.resources.Count; i++)
                    {
                        resourceList += $"{i + 1}. {node.resources[i]}\n";
                    }
                    resourcesText.text = resourceList;
                }
                else
                {
                    resourcesText.text = "No resources available";
                }
            }

            // Position panel next to node
            PositionPanel();

            // Show panel
            if (panelBackground != null)
            {
                panelBackground.SetActive(true);
                Debug.Log($"NodeInfoPanel: Panel background activated at position {transform.position}");
            }
            else
            {
                Debug.LogError("NodeInfoPanel: Panel background is NULL! Cannot show panel.");
            }

            isVisible = true;

            Debug.Log($"NodeInfoPanel: Showing info for '{node.topic}' at position {transform.position}, scale {transform.localScale}");
        }

        /// <summary>
        /// Hide the panel
        /// </summary>
        public void Hide()
        {
            if (panelBackground != null)
                panelBackground.SetActive(false);

            isVisible = false;
            targetNode = null;
        }

        /// <summary>
        /// Toggle panel visibility
        /// </summary>
        public void Toggle(Node node, Transform nodeTransform)
        {
            if (isVisible && targetNode == nodeTransform)
            {
                Hide();
            }
            else
            {
                ShowNodeInfo(node, nodeTransform);
            }
        }

        /// <summary>
        /// Check if panel is currently visible
        /// </summary>
        public bool IsVisible()
        {
            return isVisible;
        }

        /// <summary>
        /// Check if panel is showing info for a specific node
        /// </summary>
        public bool IsShowingNode(Transform nodeTransform)
        {
            return isVisible && targetNode == nodeTransform;
        }

        private void LateUpdate()
        {
            if (isVisible && targetNode != null)
            {
                PositionPanel();

                // Optionally face the camera
                if (faceCamera && mainCamera != null)
                {
                    transform.LookAt(mainCamera.transform);
                    transform.Rotate(0, 180, 0); // Flip to face camera correctly
                }
            }
        }

        /// <summary>
        /// Position the panel next to the target node
        /// </summary>
        private void PositionPanel()
        {
            if (targetNode == null) return;

            // Position panel to the side of the node
            Vector3 offset = offsetDirection.normalized * offsetDistance;
            Vector3 newPosition = targetNode.position + offset;
            transform.position = newPosition;
        }

        /// <summary>
        /// Set the offset distance from the node
        /// </summary>
        public void SetOffsetDistance(float distance)
        {
            offsetDistance = distance;
        }

        /// <summary>
        /// Set the offset direction from the node
        /// </summary>
        public void SetOffsetDirection(Vector3 direction)
        {
            offsetDirection = direction;
        }
    }
}

