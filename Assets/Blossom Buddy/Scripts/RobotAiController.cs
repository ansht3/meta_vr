// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Meta.XR.BuildingBlocks.AIBlocks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Meta.XR;
using System;

public class RobotAiController : MonoBehaviour
{
    [SerializeField, Range(0f, 2f)] private float landingXOffset = 0.3f;

    [SerializeField] private string systemPrompt =
        "You are a friendly cute assistant that just got a question. " +
        "Answer the user in a funny way that you got the order and you are working on it. But keep it in one sentence." +
        "You will go to the requested location, gather information for him and come up with a description of the object. " +
        "Answer to this prompt in maximum one sentence. Be creative everytime to switch up your answers";

    private MetaMaticAnimController _robot;
    private ObjectDetectionAgent _detector;
    private ObjectDetectionVisualizer _detectionVisualizer;
    private LlmAgent _llm;
    private SpeechToTextAgent _stt;
    private TextToSpeechAgent _tts;
    private AudioController _audio;

    private readonly float _noticingTime = 1f;
    private readonly float _thinkingTime = 3f;
    private readonly float _hoveringTime = 1f;
    private readonly float _speakingTime = 2.5f;
    private readonly float _forcedLandingDelay = 3f;

    private EnvironmentRaycastManager _depth;
    private PassthroughCameraAccess _ptCam;
    private List<BoxData> _lastDetections = new();
    private Camera _cam;
    private bool _gotDetections;
    private bool _busy;

    private void Awake()
    {
        _robot = FindAnyObjectByType<MetaMaticAnimController>();
        _stt = FindAnyObjectByType<SpeechToTextAgent>();
        _tts = FindAnyObjectByType<TextToSpeechAgent>();
        _llm = FindAnyObjectByType<LlmAgent>();
        _detector = FindAnyObjectByType<ObjectDetectionAgent>();
        _detectionVisualizer = FindAnyObjectByType<ObjectDetectionVisualizer>();
        _audio = FindAnyObjectByType<AudioController>();
        if (_audio == null && _robot) _audio = _robot.GetComponent<AudioController>(); // ensure we use the robot’s AudioSources

        if (_detector != null)
        {
            _detector.OnDetectionResponseReceived.AddListener(HandleDetections); // fires after inference :contentReference[oaicite:1]{index=1}
        }
    }

    private void OnDestroy()
    {
        if (_detector != null)
        {
            _detector.OnDetectionResponseReceived.RemoveListener(HandleDetections);
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        _cam = Camera.main;
        _ptCam = FindAnyObjectByType<PassthroughCameraAccess>();
        _depth = FindAnyObjectByType<EnvironmentRaycastManager>();

        if (!_cam || !_robot) yield break;

        var front = _cam.transform.position + _cam.transform.forward;
        front.y = _cam.transform.position.y;
        _robot.Move(front);
        _robot.Look(_cam.transform.position);
        _robot.ForceFlying();

        // make sure boxes can show if the visualizer exists
        if (_detectionVisualizer != null) _detectionVisualizer.ShowBoundingBoxes = true;
    }

    public void StartVoiceInput()
    {
        if (_busy || !_ptCam || !_ptCam.IsPlaying || _stt == null) return;

        _audio?.PlayMicStart();

        void Once(string txt)
        {
            _stt.onTranscript.RemoveListener(Once);
            _audio?.PlayMicStop();
            StartCoroutine(MainSequence(txt));
        }

        _stt.onTranscript.AddListener(Once);
        _stt.StartListening(); // new STT API :contentReference[oaicite:3]{index=3}
    }

    private IEnumerator MainSequence(string userText)
    {
        print(userText);
        if (string.IsNullOrWhiteSpace(userText)) yield break;

        _busy = true;

        // run one detection pass first; this also drives the visualizer via its own subscription
        yield return RunDetectionOnce(0.75f);
        if (_lastDetections is { Count: > 0 }) _audio?.PlayDetection();

        // 1) NOTICING (sfx + anim)
        bool right = Vector3.Dot(_robot.transform.right, _cam.transform.position - _robot.transform.position) < 0;
        if (right) _robot.TriggerNoticingRightAnimation();
        else _robot.TriggerNoticingLeftAnimation();
        _audio?.PlayNoticing(); // ensure bound to robot AudioSources (see Awake)
        yield return new WaitForSeconds(_noticingTime);

        // 2) First LLM ack (text-only)
        string firstReply = null;
        void FirstCb(string s) => firstReply = s;
        _llm.OnAssistantReply += FirstCb; // LlmAgent event :contentReference[oaicite:4]{index=4}
        _ = _llm.SendTextOnlyAsync($"System: {systemPrompt}\n\n{userText}");
        while (firstReply == null) yield return null;
        _llm.OnAssistantReply -= FirstCb;

        _robot.LerpLookAtTargetTo(_cam.transform.position, .3f);
        _robot.Look(_cam.transform.position);
        _robot.TriggerSpeakingAnimation();
        yield return SpeakAndWait(firstReply); // TTS SpeakText + onSpeakFinished :contentReference[oaicite:5]{index=5}

        // 3) Pick target object (normalize boxes to xmin/ymin/xmax/ymax before projecting)
        Vector3 objPosWorld = _robot.transform.position + _cam.transform.forward * 2f;
        string detectedLabel = null;

        if (_lastDetections is { Count: > 0 } && _detectionVisualizer != null)
        {
            var words = Regex.Split(userText, @"\W+")
                .Where(w => w.Length > 1)
                .Select(w => w.ToLowerInvariant())
                .ToArray();

            var candidates = _lastDetections.Where(b =>
            {
                var clean = Regex.Replace(b.label, @"\s\d+\.\d+$", "").ToLowerInvariant();
                return words.Any(w => clean.Contains(w));
            }).ToList();

            if (candidates.Count == 0) candidates = new List<BoxData>(_lastDetections);

            float bestDist = float.MaxValue;
            foreach (var box in candidates)
            {
                GetMinMax(box, out float xmin, out float ymin, out float xmax, out float ymax);
                if (!_detectionVisualizer.TryProject(xmin, ymin, xmax, ymax, out var world, out _, out _))
                    continue;

                var d = Vector3.Distance(_cam.transform.position, world);
                if (d < bestDist)
                {
                    bestDist = d;
                    objPosWorld = world;
                    detectedLabel = Regex.Replace(box.label, @"\s\d+\.\d+$", "");
                }
            }
        }

        yield return StartCoroutine(RobotSequence(objPosWorld, userText, detectedLabel));

        _busy = false;
    }

    private IEnumerator RobotSequence(Vector3 targetObj, string userText, string detectedLabel)
    {
        yield return EnsureFlying();

        _robot.TriggerIdleAnimation();
        var landing = ComputeLanding(targetObj);

        var rightOffset = _cam.transform.right * landingXOffset;
        landing += rightOffset;

        _robot.LandHere(landing, targetObj);

        var timer = 0f;
        while (timer < 8f && _robot._behaviorState != MetaMaticAnimController.BehaviorState.Grounded)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _audio?.PlayLanding();
        _audio?.StopFlyingLoop();
        _robot.Look(targetObj);
        yield return new WaitForSeconds(_forcedLandingDelay);
        _robot.TriggerThinkingAnimation();
        _audio?.StartThinkingLoop();

        // 2nd LLM call (prefer vision)
        string secondReply = null;
        void SecondCb(string s) => secondReply = s;
        _llm.OnAssistantReply += SecondCb;

        var prompt = string.IsNullOrEmpty(detectedLabel) ? userText
            : $"{userText}\n\nThe detected object looks like a {detectedLabel}. Only answer about this object and in " +
              $"one sentence. It should be a useful information, and be funny. Don't answer to this: Keep in mind that " +
              $"the object in question may not be accurately described. The user may say tv and it is a PC screen, " +
              $"or he may say dining table but it might be a coffee table.";

        _ = _llm.SendPromptWithPassthroughImageAsync(prompt); // falls back to text if camera not available :contentReference[oaicite:6]{index=6}

        float t = 0;
        while (t < _thinkingTime || secondReply == null)
        {
            t += Time.deltaTime;
            yield return null;
        }

        _llm.OnAssistantReply -= SecondCb;

        _audio?.StopThinkingLoop();
        _robot.TriggerIdleAnimation();
        _robot.LerpLookAtTargetTo(_cam.transform.position, .3f);
        _robot.Look(_cam.transform.position);
        _robot.TriggerSpeakingAnimation();

        if (!string.IsNullOrEmpty(secondReply)) yield return SpeakAndWait(secondReply);
        else yield return new WaitForSeconds(_speakingTime);

        _robot.TriggerTakeoffAnimation();
        _robot.TriggerIdleAnimation();
        _robot.ForceFlying();

        var hover = _cam.transform.position + _cam.transform.forward;
        hover.y = _cam.transform.position.y;
        for (t = 0; t < _hoveringTime; t += Time.deltaTime)
        {
            _robot.Move(hover);
            _robot.Look(_cam.transform.position);
            yield return null;
        }

        _robot.Move(hover);
        _robot.Look(_cam.transform.position);
    }

    private IEnumerator EnsureFlying()
    {
        switch (_robot._behaviorState)
        {
            case MetaMaticAnimController.BehaviorState.Flying:
                _audio?.StartFlyingLoop();
                yield break;
            case MetaMaticAnimController.BehaviorState.Grounded:
                _robot.TriggerTakeoffAnimation();
                break;
            case MetaMaticAnimController.BehaviorState.MovingForLanding:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var t = 0f;
        while (t < 3f && _robot._behaviorState != MetaMaticAnimController.BehaviorState.Flying)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (_robot._behaviorState != MetaMaticAnimController.BehaviorState.Flying)
            _robot.ForceFlying();
    }

    private Vector3 ComputeLanding(Vector3 targetCentre)
    {
        var camPos = _cam.transform.position;

        float[] radii = { 0.4f, 0.6f, 0.8f };
        const int steps = 12;
        const float maxRay = 6f;
        const float slope = 25f;
        const float minSide = 0.3f;
        const float yMerge = 0.08f;

        var hits = new List<(Vector3 pt, float dist)>();

        foreach (var r in radii)
        {
            for (var i = 0; i < steps; i++)
            {
                var ang = i * Mathf.PI * 2f / steps;
                var offset = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * r;
                var sample = targetCentre + offset;
                var dir = (sample - camPos).normalized;
                var ray = new Ray(camPos, dir);

                var hit = false;
                var hitPt = Vector3.zero;
                var hitNm = Vector3.up;
                var dist = 0f;

                if (_depth && _depth.Raycast(ray, out var dHit, maxRay))
                {
                    hit = true;
                    hitPt = dHit.point;
                    hitNm = dHit.normal;
                    dist = Vector3.Distance(camPos, hitPt);
                }
                else if (Physics.Raycast(ray, out var pHit, maxRay))
                {
                    hit = true;
                    hitPt = pHit.point;
                    hitNm = pHit.normal;
                    dist = pHit.distance;
                }

                if (!hit) continue;
                if (Vector3.Angle(hitNm, Vector3.up) > slope) continue;
                if (Vector3.Distance(hitPt, targetCentre) < minSide) continue;

                hits.Add((hitPt, dist));
            }
        }

        if (hits.Count == 0) return targetCentre;

        hits.Sort((a, b) => a.pt.y.CompareTo(b.pt.y));
        var clusters = new List<List<(Vector3 pt, float dist)>>();

        foreach (var h in hits)
        {
            var placed = false;
            foreach (var cl in clusters)
            {
                if (Mathf.Abs(cl[0].pt.y - h.pt.y) < yMerge)
                {
                    cl.Add(h);
                    placed = true;
                    break;
                }
            }

            if (!placed) clusters.Add(new List<(Vector3 pt, float dist)> { h });
        }

        clusters.Sort((a, b) =>
        {
            var diff = b.Count.CompareTo(a.Count);
            if (diff != 0) return diff;
            var da = a.Min(v => Vector3.Distance(v.pt, targetCentre));
            var db = b.Min(v => Vector3.Distance(v.pt, targetCentre));
            return da.CompareTo(db);
        });

        var bestCluster = clusters[0];
        var bestHit = bestCluster.OrderBy(v => v.dist).First().pt;

        return bestHit;
    }

    private void HandleDetections(List<BoxData> boxes)
    {
        _lastDetections = boxes ?? new List<BoxData>();
        _gotDetections = true;
    }

    private IEnumerator RunDetectionOnce(float timeoutSeconds)
    {
        _gotDetections = false;
        _lastDetections = new List<BoxData>();
        if (_detector) _detector.CallInference(); // async, results via event :contentReference[oaicite:7]{index=7}

        var t = 0f;
        while (!_gotDetections && t < Mathf.Max(0.05f, timeoutSeconds))
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator SpeakAndWait(string text)
    {
        if (!_tts || string.IsNullOrWhiteSpace(text))
        {
            yield return new WaitForSeconds(_speakingTime);
            yield break;
        }

        var done = false;
        void Done() => done = true;

        _tts.onSpeakFinished.AddListener(Done); // wait until playback completes :contentReference[oaicite:8]{index=8}
        _tts.SpeakText(text);

        var guard = 0f;
        while (!done && guard < 30f)
        {
            guard += Time.deltaTime;
            yield return null;
        }

        _tts.onSpeakFinished.RemoveListener(Done);
    }

    private static void GetMinMax(in BoxData b, out float xmin, out float ymin, out float xmax, out float ymax)
    {
        // Some providers return (x, y, w, h). Others return (xmin, ymin, xmax, ymax).
        xmin = b.position.x;
        ymin = b.position.y;
        xmax = b.scale.x;
        ymax = b.scale.y;

        // If the "max" is not actually greater than "min", assume (x, y, w, h) and convert.
        if (xmax <= xmin || ymax <= ymin)
        {
            xmax = xmin + b.scale.x;
            ymax = ymin + b.scale.y;
        }
    }
}
