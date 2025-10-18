using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MetaMaticMockcupGameplay : MonoBehaviour
{
    [SerializeField] private MetaMaticAnimController _metamaticAnimController;
    private Vector3 _goal;
    private Vector3 _lookAt;
    [SerializeField] Vector3 _centerOfRandomMovingArea = new Vector3(0, 0, 0);
    [SerializeField] float _areaSize = 1.0f;

    void Awake()
    {
        if (!_metamaticAnimController)
        {
            _metamaticAnimController = FindObjectOfType<MetaMaticAnimController>();
            Debug.LogWarning("No MetaMaticAnimController assigned, using the first one found in the scene");
        }
    }

    void Update()
    {
        DebugInputs();
    }

    private void DebugInputs()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _goal = (Random.insideUnitSphere * _areaSize) + _centerOfRandomMovingArea;
            _lookAt = _goal + _metamaticAnimController.transform.forward;
            _metamaticAnimController.LerpLookAtTargetTo(_lookAt, 0.5f);
        }

        if (Keyboard.current.spaceKey.isPressed)
        {
            _metamaticAnimController.Move(_goal);
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            //_metamaticAnimController.Look(Camera.main.transform.position);
        }

        if (Keyboard.current.numpad0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame)
        {
            _metamaticAnimController.LerpLookAtTargetTo(Vector3.zero, 0.5f);
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerListeningAnimation();
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerHearingAnimation();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerThinkingAnimation();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerSpeakingAnimation();
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerIdleAnimation();
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerLandingAnimation();
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerTakeoffAnimation();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // if grounded, lift first
                if (_metamaticAnimController._behaviorState == MetaMaticAnimController.BehaviorState.Grounded)
                    _metamaticAnimController.TriggerTakeoffAnimation();

                _metamaticAnimController.LandHere(hit.point, Vector3.zero);
            }
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerNoticingLeftAnimation();
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            _metamaticAnimController.TriggerNoticingRightAnimation();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_metamaticAnimController._landingStartPosition, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            _metamaticAnimController._landingStartPosition - (Vector3.up * _metamaticAnimController._landingAltitude),
            0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_centerOfRandomMovingArea, _areaSize);
    }
}
