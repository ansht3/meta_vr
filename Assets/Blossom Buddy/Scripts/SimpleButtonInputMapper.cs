using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SimpleButtonInputMapper : MonoBehaviour
{
    [Header("Bind your methods here")]
    [SerializeField] private UnityEvent onButtonOneDown;
    [SerializeField] private UnityEvent onButtonTwoDown;
    [SerializeField] private UnityEvent onButtonThreeDown;
    [SerializeField] private UnityEvent onButtonFourDown;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip[] buttonSounds = new AudioClip[4]; // Different sounds for each button

    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugMessages = true;
    
    [Header("Testing")]
    [SerializeField] private bool enableMouseTesting = true;

    private RobotAiController _robotAiController;
    private LearningGraphAssistant _learningGraphAssistant;

    private void Start()
    {
        Debug.Log("[ButtonInputMapper] Starting up...");
        
        // Try multiple ways to find the LearningGraphAssistant
        _learningGraphAssistant = FindAnyObjectByType<LearningGraphAssistant>();
        
        if (_learningGraphAssistant == null)
        {
            Debug.LogWarning("[ButtonInputMapper] LearningGraphAssistant not found with FindAnyObjectByType!");
            
            // Try alternative search methods
            var allObjects = FindObjectsOfType<LearningGraphAssistant>();
            Debug.Log($"[ButtonInputMapper] Found {allObjects.Length} LearningGraphAssistant objects in scene");
            
            if (allObjects.Length > 0)
            {
                _learningGraphAssistant = allObjects[0];
                Debug.Log("[ButtonInputMapper] Using first found LearningGraphAssistant");
            }
        }
        
        if (_learningGraphAssistant == null)
        {
            Debug.LogError("[ButtonInputMapper] LearningGraphAssistant still not found! Make sure:");
            Debug.LogError("1. GameObject with LearningGraphAssistant component exists in scene");
            Debug.LogError("2. GameObject is active (not disabled)");
            Debug.LogError("3. Component is enabled");
        }
        else
        {
            Debug.Log($"[ButtonInputMapper] LearningGraphAssistant found and connected! GameObject: {_learningGraphAssistant.gameObject.name}");
        }
        
        // Auto-find AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = FindAnyObjectByType<AudioSource>();
            }
        }
        
        Debug.Log($"[ButtonInputMapper] AudioSource found: {audioSource != null}");
        Debug.Log($"[ButtonInputMapper] Show debug messages: {showDebugMessages}");
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            PlayButtonFeedback("A Button - Learning Graph", 0);
            _learningGraphAssistant?.StartVoiceCapture();
            onButtonOneDown?.Invoke();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            PlayButtonFeedback("B Button", 1);
            onButtonTwoDown?.Invoke();
        }
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            PlayButtonFeedback("Grip Button", 2);
            onButtonThreeDown?.Invoke();
        }

        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            PlayButtonFeedback("Trigger Button", 3);
            onButtonFourDown?.Invoke();
        }

        // Mouse testing (for development without Quest)
        if (enableMouseTesting)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlayButtonFeedback("A Button - Learning Graph (Mouse Test)", 0);
                onButtonOneDown?.Invoke();
                _learningGraphAssistant?.StartVoiceCapture();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                PlayButtonFeedback("B Button (Mouse Test)", 1);
                onButtonTwoDown?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                PlayButtonFeedback("Grip Button (Mouse Test)", 2);
                onButtonThreeDown?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                PlayButtonFeedback("Trigger Button (Mouse Test)", 3);
                onButtonFourDown?.Invoke();
            }
        }
    }

    private void PlayButtonFeedback(string buttonName, int buttonIndex)
    {
        // Visual feedback - debug message (always show for troubleshooting)
        Debug.Log($"[ButtonInputMapper] {buttonName} pressed! (Index: {buttonIndex})");
        
        if (showDebugMessages)
        {
            Debug.Log($"[ButtonInputMapper] Debug messages enabled - {buttonName} action triggered!");
        }

        // Audio feedback
        if (audioSource != null)
        {
            AudioClip soundToPlay = null;
            
            // Try to use specific sound for this button, fallback to general sound
            if (buttonSounds != null && buttonIndex < buttonSounds.Length && buttonSounds[buttonIndex] != null)
            {
                soundToPlay = buttonSounds[buttonIndex];
            }
            else if (buttonClickSound != null)
            {
                soundToPlay = buttonClickSound;
            }

            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }

        // Visual feedback - screen flash (optional)
        StartCoroutine(FlashScreen());
    }

    private IEnumerator FlashScreen()
    {
        // Simple screen flash effect
        Color originalColor = Camera.main.backgroundColor;
        Camera.main.backgroundColor = Color.white;
        yield return new WaitForSeconds(0.1f);
        Camera.main.backgroundColor = originalColor;
    }
}
