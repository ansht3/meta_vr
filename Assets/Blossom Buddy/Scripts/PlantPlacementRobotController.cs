// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Meta.XR;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LineRenderer))]
public class PlantPlacementRobotController : MonoBehaviour
{
    [SerializeField] private MetaMaticAnimController robot;
    [SerializeField] private Transform raycastHand;
    [SerializeField] private GameObject[] plantPrefabs;

    private float _noticingTime = 1f;
    private float _thinkingTime = 3f;
    private float _hoveringTime = 1f;
    private float _speakingTime = 2.5f;
    private float _plantLandingOffset = 0.8f;
    private float _placementHeightOffset = 0.03f;

    [Header("Debug")]
    [SerializeField] private bool allowMouse;

    private const float MaxRay = 5f;
    private bool _busy;
    private Camera _cam;
    private Coroutine _seq;
    private LineRenderer _line;
    private AudioController _audioController;
    private EnvironmentRaycastManager _depth;

    private void Awake()
    {
        _cam = Camera.main;
        _depth = FindAnyObjectByType<EnvironmentRaycastManager>();
        _audioController = FindAnyObjectByType<AudioController>();
        if (_audioController == null && _audioController)
        {
            _audioController = robot.GetComponent<AudioController>();
        }

        if (!robot)
        {
            robot = FindAnyObjectByType<MetaMaticAnimController>();
        }

        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2;
    }

    private void Start()
    {
        var front = _cam.transform.position + _cam.transform.forward;
        front.y = _cam.transform.position.y;
        robot.Move(front);
        robot.Look(_cam.transform.position);
    }

    private void Update()
    {
        DrawLaser();

        if (raycastHand && OVRInput.GetDown(OVRInput.Button.One))
        {
            TryPlace(raycastHand.position, raycastHand.forward);
        }

        if (allowMouse && Mouse.current.leftButton.wasPressedThisFrame)
        {
            var r = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            TryPlace(r.origin, r.direction);
        }

        // keep looking at the headset while airborne
        if (robot._behaviorState != MetaMaticAnimController.BehaviorState.Grounded)
        {
            robot.Look(_cam.transform.position);
        }
    }

    private void DrawLaser()
    {
        if (!raycastHand)
        {
            _line.enabled = false;
            return;
        }

        _line.enabled = true;
        Ray ray = new Ray(raycastHand.position, raycastHand.forward);
        Vector3 end = raycastHand.position + raycastHand.forward * MaxRay;

        if (_depth && _depth.Raycast(ray, out var dh, MaxRay)) end = dh.point;
        else if (Physics.Raycast(ray, out var ph, MaxRay)) end = ph.point;

        _line.SetPosition(0, raycastHand.position);
        _line.SetPosition(1, end);
    }

    private void TryPlace(Vector3 origin, Vector3 dir)
    {
        CancelSequence(); // interrupt any current task

        if (_depth && _depth.Raycast(new Ray(origin, dir), out var dh, MaxRay))
            SpawnAndRun(dh.point, dh.normal);
        else if (Physics.Raycast(origin, dir, out var ph, MaxRay))
            SpawnAndRun(ph.point, ph.normal);
        else Debug.LogWarning("no hit");
    }

    private void SpawnAndRun(Vector3 point, Vector3 normal)
    {
        if (_busy || plantPrefabs.Length == 0) return;

        Instantiate(
            plantPrefabs[Random.Range(0, plantPrefabs.Length)],
            point + Vector3.up * _placementHeightOffset,
            Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, normal), normal));

        _audioController?.PlayPlantPlacement();

        _seq = StartCoroutine(RobotSequence(point));
    }

    IEnumerator RobotSequence(Vector3 flower)
    {
        _busy = true;

        /* 1 – notice */
        bool right = Vector3.Dot(flower - robot.transform.position, robot.transform.right) > 0;
        _audioController?.PlayNoticing();
        if (right) robot.TriggerNoticingRightAnimation();
        else robot.TriggerNoticingLeftAnimation();
        yield return new WaitForSeconds(_noticingTime);

        /* 2 – be sure we’re in the air */
        yield return EnsureFlying();

        /* 3 – **go Idle, then land**  (critical) */
        robot.TriggerIdleAnimation(); // <<< restored
        Vector3 landing = ComputeLanding(flower);
        robot.LandHere(landing, flower);

        _audioController?.StartFlyingLoop();
        // wait until grounded, but bail after 8 s to avoid dead-lock
        const float maxWait = 8f;
        float timer = 0f;
        while (timer < maxWait &&
               robot._behaviorState != MetaMaticAnimController.BehaviorState.Grounded)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _audioController?.StopFlyingLoop();
        _audioController?.PlayLanding();

        /* 4 – think */
        robot.Look(flower);
        yield return new WaitForSeconds(3); // Force waiting for landing
        robot.TriggerThinkingAnimation();
        _audioController?.StartThinkingLoop();
        yield return new WaitForSeconds(_thinkingTime);
        _audioController?.StopThinkingLoop();
        robot.TriggerIdleAnimation();

        /* 5 – speak */
        robot.LerpLookAtTargetTo(_cam.transform.position, .3f);
        robot.TriggerSpeakingAnimation();
        yield return new WaitForSeconds(_speakingTime);

        /* 6 – take-off & hover */
        robot.TriggerTakeoffAnimation();
        yield return EnsureFlying();

        Vector3 hover = _cam.transform.position + _cam.transform.forward;
        hover.y = _cam.transform.position.y;

        float t = 0;
        while (t < _hoveringTime)
        {
            t += Time.deltaTime;
            robot.Move(hover);
            robot.Look(_cam.transform.position);
            yield return null;
        }

        robot.TriggerIdleAnimation();
        _busy = false;
        _seq = null;
    }

    public IEnumerator EnsureFlying()
    {
        switch (robot._behaviorState)
        {
            case MetaMaticAnimController.BehaviorState.Flying: yield break;
            case MetaMaticAnimController.BehaviorState.Grounded: robot.TriggerTakeoffAnimation(); break;
            case MetaMaticAnimController.BehaviorState.MovingForLanding: break;
        }

        const float max = 3f;
        float t = 0f;
        while (t < max && robot._behaviorState != MetaMaticAnimController.BehaviorState.Flying)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (robot._behaviorState != MetaMaticAnimController.BehaviorState.Flying)
        {
            robot.ForceFlying();
        }
    }

    public Vector3 ComputeLanding(Vector3 flower)
    {
        Vector3 dir = flower - _cam.transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < .001f) dir = Vector3.back;
        dir.Normalize();

        Vector3 flat = flower + dir * _plantLandingOffset;
        Vector3 probe = flat + Vector3.up * 2f;

        if (_depth && _depth.Raycast(new Ray(probe, Vector3.down), out var dh, 4f)) return dh.point;
        if (Physics.Raycast(probe, Vector3.down, out var ph, 4f)) return ph.point;

        flat.y = flower.y;
        return flat;
    }

    private void CancelSequence()
    {
        if (_seq != null)
        {
            StopCoroutine(_seq);
            _seq = null;
        }

        _busy = false;

        // put Meta-Matic airborne & idling so the next seq starts clean
        if (robot)
        {
            robot.TriggerIdleAnimation();
            robot.ForceFlying();
        }
    }

    private void OnDestroy() => CancelSequence();
}
