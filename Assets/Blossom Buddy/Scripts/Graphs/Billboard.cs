using UnityEngine;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Makes an object always face the camera
    /// Useful for text labels above nodes
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool lockX = false;
        [SerializeField] private bool lockY = false;
        [SerializeField] private bool lockZ = false;

        private Camera mainCamera;

        private void Start()
        {
            // Get the main camera
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                Debug.LogWarning("Billboard: No main camera found in scene");
            }
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // Make the object face the camera
            Vector3 targetPosition = mainCamera.transform.position;
            
            // Apply axis locks if needed
            if (lockX) targetPosition.x = transform.position.x;
            if (lockY) targetPosition.y = transform.position.y;
            if (lockZ) targetPosition.z = transform.position.z;

            // Look at the camera
            transform.LookAt(targetPosition);
            
            // Flip 180 degrees so text faces the camera correctly
            transform.Rotate(0, 180, 0);
        }
    }
}

