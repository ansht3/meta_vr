// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;
using Meta.XR.BuildingBlocks.AIBlocks;

public class RobotShowcaseController : MonoBehaviour
{
    [SerializeField] private MetaMaticAnimController robot;
    [SerializeField] private TextToSpeechAgent tts;
    [SerializeField] private float nearDistance = 1.0f;
    [SerializeField] private float farDistance = 1.5f;

    private Camera _cam;

    private void Awake()
    {
        if (!robot)
        {
            robot = FindAnyObjectByType<MetaMaticAnimController>();
        }

        if (!tts)
        {
            tts = FindAnyObjectByType<TextToSpeechAgent>();
        }
    }

    private void OnEnable()
    {
        _cam = Camera.main;
        if (!_cam || !robot)
        {
            return;
        }

        // Start 1.5m in front, level with head
        var p = _cam.transform.position + _cam.transform.forward * farDistance;
        p.y = _cam.transform.position.y;
        robot.Move(p);
        robot.ForceFlying();

        if (!tts)
        {
            return;
        }

        tts.onSpeakStarting.AddListener(OnSpeakStart);
        tts.onSpeakFinished.AddListener(OnSpeakEnd);
    }

    private void OnDisable()
    {
        if (!tts)
        {
            return;
        }

        tts.onSpeakStarting.RemoveListener(OnSpeakStart);
        tts.onSpeakFinished.RemoveListener(OnSpeakEnd);
    }

    private void Update()
    {
        if (!_cam || !robot)
        {
            return;
        }

        // Always look at the user
        robot.Look(_cam.transform.position);

        // Maintain comfortable band [nearDistance, farDistance]
        var toRobot = robot.transform.position - _cam.transform.position;
        var flatDir = Vector3.ProjectOnPlane(toRobot, Vector3.up);
        var dist = flatDir.magnitude;

        if (!(dist < nearDistance) && !(dist > farDistance))
        {
            return;
        }

        var target = _cam.transform.position + _cam.transform.forward * farDistance;
        target.y = _cam.transform.position.y;
        robot.Move(target);
    }

    private void OnSpeakStart(string _)
    {
        if (robot)
        {
            robot.TriggerSpeakingAnimation();
        }
    }

    private void OnSpeakEnd()
    {
        if (robot)
        {
            robot.TriggerIdleAnimation();
        }
    }
}
