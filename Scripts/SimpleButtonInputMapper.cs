using UnityEngine;
using UnityEngine.Events;

public class SimpleButtonInputMapper : MonoBehaviour
{
    [Header("Bind your methods here")]
    [SerializeField] private UnityEvent onButtonOneDown;
    [SerializeField] private UnityEvent onButtonTwoDown;
    [SerializeField] private UnityEvent onButtonThreeDown;
    [SerializeField] private UnityEvent onButtonFourDown;

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            onButtonOneDown?.Invoke();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            onButtonTwoDown?.Invoke();
        }
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            onButtonThreeDown?.Invoke();
        }

        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            onButtonFourDown?.Invoke();
        }
    }
}