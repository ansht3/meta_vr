using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Serialization;
using UnityEngine.Animations;
using System;
using UnityEngine.XR;
using System.Collections;
using UnityEditor;
using System.Threading;

public class MetaMaticAnimController : MonoBehaviour
{
    public enum BehaviorState
    {
        Flying,
        MovingForLanding,
        Grounded
    };

    public BehaviorState _behaviorState = BehaviorState.Flying;
    private Animator _animator;
    [Header("Move and Look")] public Transform _lookAtTarget;
    [SerializeField] private Transform _goalTransform;

    [Range(0f, 1f)]
    [Tooltip("0: the pitch will be use only according to accelleration, 1: pitch will be used to look at the target")]
    [SerializeField]
    private float _pitchToLookAtTarget = 0.5f;

    [HideInInspector] public float _pitchToLookAtTargeBeforeLanding = 0.5f;
    private Vector3 _landingLookAtTarget;

    [HideInInspector]
    public float
        _audioInput; // plug here the normalized audio input, which might need some filtering if too abrupt in its fluctuations

    [Header("Speaking")]
    [SerializeField] Renderer _emissiveSpeakingRenderer;
    [SerializeField] private float _speakingBrightnessMultiplier = 1.5f;
    [SerializeField] private bool _simulateMetaMaticAudioSignal = false;
    [SerializeField] private GameObject _disableOnThinking, _enableOnThinking;
    private bool _simulatingAudioSignal = false;
    private Color _defaultColor;
    private Vector3 _defaultColorHSV;
    public static int _WakeUp = Animator.StringToHash("WakeUp");
    public static int _Thinking = Animator.StringToHash("Thinking");
    public static int _Hearing = Animator.StringToHash("Hearing");

    public static int _Speaking = Animator.StringToHash("Speaking");
    public static int _Idle = Animator.StringToHash("Idle");
    public static int _Land = Animator.StringToHash("Land");
    public static int _Takeoff = Animator.StringToHash("Takeoff");
    public static int _NoticingLeft = Animator.StringToHash("NoticingLeft");
    public static int _NoticingRight = Animator.StringToHash("NoticingRight");


    public enum ThinkingInterface
    {
        Eyes,
        SpriteRenderer
    };

    [Header("Thinking")] public ThinkingInterface _thinkingInterface = ThinkingInterface.Eyes;
    [SerializeField] private Animator _spriteRendererAnimator;
    [SerializeField] private AudioSource _sfxAudioSource;
    [SerializeField] AudioClip[] _thinkingAudioClips;

    [HideInInspector] public MetaMaticTrajectory _metaMaticTrajectory;
    float _initialFloatAmplitude;
    private Coroutine _lerpLookAtTargetCoroutine;

    [Header("Landing")] public float _landingAltitude = 0.93f;
    public Vector3 _landingStartPosition;


    void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRendererAnimator.gameObject.SetActive(false);
        _defaultColorHSV = ToHSV(_defaultColor);
        _metaMaticTrajectory = GetComponentInParent<MetaMaticTrajectory>();
        _initialFloatAmplitude = _metaMaticTrajectory._floatAmplitude;
        if (!_metaMaticTrajectory)
        {
            Debug.LogError("No MetaMaticTrajectory found in the parent of the MetaMaticAnimController");
        }
    }

    void Update()
    {
        AudioSignalSimulation();
        if (_behaviorState == BehaviorState.Grounded)
        {
            _metaMaticTrajectory.enabled = false;

            // 1. build a horizontal vector toward the target
            Vector3 toTarget = _lookAtTarget.position - transform.position;
            toTarget.y = 0f;                                  // <-- remove pitch component
            if (toTarget.sqrMagnitude < 0.0001f)              // fallback if we’re on top of the target
                toTarget = transform.forward;

            // 2. yaw-only orientation (up axis is world-up)
            Quaternion targetYaw = Quaternion.LookRotation(toTarget, Vector3.up);

            // 3. smooth it in
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetYaw,
                Time.deltaTime * 5f);                         // tweak speed to taste
        }

        else
        {
            _metaMaticTrajectory.enabled = true;
            Vector3 fwd = _lookAtTarget.position - transform.position;
            if (fwd.sqrMagnitude < 0.0001f) fwd = transform.forward;

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(fwd, Vector3.up),   // <-- up is always world-up
                _pitchToLookAtTarget);
        }

        if (_behaviorState == BehaviorState.MovingForLanding)
        {
            LandingLogic();
        }

        if (_audioInput > 0f)
        {
            float remappedAudioInput = math.remap(0f, 1f, 0f, .75f, _audioInput);
            _emissiveSpeakingRenderer.material.SetColor("_EmissionColor",
                Color.HSVToRGB(_defaultColorHSV.x, _defaultColorHSV.y,
                    remappedAudioInput * _speakingBrightnessMultiplier));
        }
    }

    public void TriggerListeningAnimation()
    {
        _animator.SetTrigger(_WakeUp);
        _simulatingAudioSignal = false;
        if (_thinkingInterface == ThinkingInterface.SpriteRenderer)
        {
            SwitchThinkingSpriteRendererOff();
        }

        StopSfx();
    }

    public void TriggerHearingAnimation()
    {
        _animator.SetTrigger(_Hearing);
        _simulatingAudioSignal = false;
        if (_thinkingInterface == ThinkingInterface.SpriteRenderer)
        {
            SwitchThinkingSpriteRendererOff();
        }
    }

    public void TriggerThinkingAnimation()
    {
        _animator.SetTrigger(_Thinking);
        _simulatingAudioSignal = false;
        if (_thinkingInterface == ThinkingInterface.SpriteRenderer)
        {
            SwitchThinkingSpriteRendererOn();
        }

        //PlayThinkingSfx();
    }

    public void TriggerSpeakingAnimation()
    {
        _animator.SetTrigger(_Speaking);
        _simulatingAudioSignal = true;
        if (_thinkingInterface == ThinkingInterface.SpriteRenderer)
        {
            SwitchThinkingSpriteRendererOff();
        }

        StopSfx();
    }

    public void TriggerIdleAnimation()
    {
        _animator.SetTrigger(_Idle);
        _simulatingAudioSignal = false;
        if (_thinkingInterface == ThinkingInterface.SpriteRenderer)
        {
            SwitchThinkingSpriteRendererOff();
        }

        StopSfx();
    }

    public void TriggerNoticingLeftAnimation()
    {
        _animator.SetTrigger(_NoticingLeft);
        _simulatingAudioSignal = false;
        if (_thinkingInterface == ThinkingInterface.SpriteRenderer)
        {
            SwitchThinkingSpriteRendererOff();
        }

        StopSfx();
    }

    public void TriggerNoticingRightAnimation()
    {
        _animator.SetTrigger(_NoticingRight);
        _simulatingAudioSignal = false;
        if (_thinkingInterface == ThinkingInterface.SpriteRenderer)
        {
            SwitchThinkingSpriteRendererOff();
        }

        StopSfx();
    }

    public void TriggerLandingAnimation()
    {
        _animator.SetTrigger(_Land);
        _simulatingAudioSignal = false;
        StartCoroutine(_metaMaticTrajectory.BringFloatAmplitudeTo(0f, .5f));
        if (_thinkingInterface == ThinkingInterface.SpriteRenderer)
        {
            SwitchThinkingSpriteRendererOff();
        }

        StopSfx();
    }

    public void TriggerTakeoffAnimation()
    {
        _animator.SetTrigger(_Takeoff);
        StartCoroutine(_metaMaticTrajectory.BringFloatAmplitudeTo(_initialFloatAmplitude, .5f));
        StartCoroutine(EnsureAirborne());
    }

    private IEnumerator EnsureAirborne()
    {
        const float maxWait = 3f; // seconds
        float timer = 0;
        // wait until normal animation event sets state
        while (_behaviorState != BehaviorState.Flying && timer < maxWait)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (_behaviorState != BehaviorState.Flying) // event never arrived
        {
            Debug.LogWarning("[MetaMatic] Take-off event missed – forcing Flying state.");
            ForceFlying();
        }
    }

    // add anywhere inside MetaMaticAnimController (public section)
    public void ForceFlying()
    {
        if (_behaviorState != BehaviorState.Flying)
        {
            _behaviorState = BehaviorState.Flying;
            _metaMaticTrajectory._rb.isKinematic = false;
        }
    }

    public void TriggerTakeoffAndLand(Vector3 andLandHere)
    {
        _animator.SetTrigger(_Takeoff);
        print("about to start the coroutine to wait to be airborne");
        StartCoroutine(LandHereOnceAirborne(andLandHere));
    }

    IEnumerator LandHereOnceAirborne(Vector3 andLandHere)
    {
        yield return new WaitUntil(() => _behaviorState == BehaviorState.Flying);
        LandHere(andLandHere, _landingLookAtTarget);
    }

    public void RecenterContainerToModel() //Called from animation event to recenter the container to the model
    {
        Transform container = transform.parent;
        this.transform.parent = null;
        _metaMaticTrajectory._goal.position = this.transform.position;
        container.position = this.transform.position;
        this.transform.parent = container;
    }

    void SwitchThinkingSpriteRendererOn()
    {
        _disableOnThinking.SetActive(false);
        _enableOnThinking.SetActive(true);
    }

    void SwitchThinkingSpriteRendererOff()
    {
        _disableOnThinking.SetActive(true);
        _enableOnThinking.SetActive(false);
    }

    /// <summary>
    /// Updates the position of the look at target.
    /// </summary>
    /// <param name="targetPosition"></param>
    public void Look(Vector3 targetPosition)
    {
        _lookAtTarget.position = targetPosition;
    }

    /// <summary>
    /// Called in the Update function, will make goal position move towards the newGoalPosition.
    /// </summary>
    /// <param name="newGoalPosition"></param>
    public void Move(Vector3 newGoalPosition)
    {
        float dist = Vector3.Distance(newGoalPosition, transform.position);
        if (dist < 0.1f)
        {
            return;
        }

        _goalTransform.position += (newGoalPosition - _goalTransform.position).normalized * Time.deltaTime * 2f;
    }

    /// <summary>
    /// Called to make goal position move towards the newGoalPosition smoothly.
    /// </summary>
    /// <param name="newGoalPosition"></param>
    /// <param name="lerpTime"></param>
    public void LerpLookAtTargetTo(Vector3 newGoalPosition, float lerpTime)
    {
        if (_lerpLookAtTargetCoroutine != null)
        {
            StopCoroutine(_lerpLookAtTargetCoroutine);
        }

        _lerpLookAtTargetCoroutine = StartCoroutine(LerpLookAtTargetCoroutine(newGoalPosition, lerpTime));
    }

    IEnumerator LerpLookAtTargetCoroutine(Vector3 newGoalPosition, float lerpTime)
    {
        float timer = 0f;
        while (timer < lerpTime)
        {
            timer += Time.deltaTime;
            _lookAtTarget.position = Vector3.Lerp(_lookAtTarget.position, newGoalPosition, timer / lerpTime);
            yield return null;
        }

        _lerpLookAtTargetCoroutine = null;
    }

    /// <summary>
    /// Sends MetaMatic to land at the requested spot.
    /// If already grounded, it first takes off, then lands at the new spot.
    /// </summary>
    public void LandHere(Vector3 landingSpot, Vector3 lookingTowards)
    {
        // Already on the ground? → lift off, then land at the new place
        if (_behaviorState == BehaviorState.Grounded)
        {
            TriggerTakeoffAndLand(landingSpot);
            return;
        }

        _landingLookAtTarget = lookingTowards;
        _landingStartPosition = landingSpot + (Vector3.up * _landingAltitude);
        _metaMaticTrajectory._goal.position = _landingStartPosition;
        _metaMaticTrajectory._goalLookAt.position = landingSpot;

        _behaviorState = BehaviorState.MovingForLanding;
        _pitchToLookAtTargeBeforeLanding = _pitchToLookAtTarget;
    }

    void LandingLogic() //Called in the Update function in the state is MovingForLanding
    {
        StartCoroutine(_metaMaticTrajectory.BringFloatAmplitudeTo(0f, .5f));
        _goalTransform.position = Vector3.Lerp(_goalTransform.position, _landingStartPosition, .5f);
        Move(_landingStartPosition);
        float distFromLandingStartPos = Vector3.Distance(_landingStartPosition, transform.position);
        _pitchToLookAtTarget = math.remap(0f, 1f, 0f, _pitchToLookAtTargeBeforeLanding, distFromLandingStartPos);
        _lookAtTarget.position = Vector3.Lerp(_landingLookAtTarget, _lookAtTarget.position, distFromLandingStartPos);
        if (distFromLandingStartPos < 0.01f && _metaMaticTrajectory._rb.linearVelocity.magnitude < 0.05f)
        {
            Vector3 lastVelocity = _metaMaticTrajectory._rb.linearVelocity;
            _metaMaticTrajectory._rb.isKinematic = true;
            StartCoroutine(LerpToExactPositionToStartLanding(lastVelocity, .5f));
            StartCoroutine(LerpContainerToBePerfectlyStraight(1f));
        }
    }

    IEnumerator LerpToExactPositionToStartLanding(Vector3 startingVel, float decellerationMult)
    {
        {
            Vector3 newVel = startingVel;
            while (Vector3.Distance(transform.position, _landingStartPosition) > 0.001f && newVel.magnitude > 0.001f)
            {
                newVel = Vector3.Lerp(newVel,
                    (_landingStartPosition - transform.position).normalized * newVel.magnitude, .5f);
                print("newVelmag:" + newVel.magnitude);
                transform.position += newVel * Time.deltaTime;
                _metaMaticTrajectory._goal.position = transform.position + Vector3.up;
                yield return null;
            }

            transform.position = _landingStartPosition;
            print("reached landing position:" + transform.position);
            _metaMaticTrajectory._goal.position = transform.position + Vector3.up;
            TriggerLandingAnimation();
            _behaviorState = BehaviorState.Grounded;
        }
    }

    IEnumerator LerpContainerToBePerfectlyStraight(float isSeconds)
    {
        float timer = 0f;
        while (timer < isSeconds)
        {
            timer += Time.deltaTime;
            _metaMaticTrajectory.transform.rotation = Quaternion.Lerp(_metaMaticTrajectory.transform.rotation,
                Quaternion.LookRotation(Vector3.up, _metaMaticTrajectory.transform.up), timer / isSeconds);
            yield return null;
        }
    }

    public void
        SetBehaviorStateToFlying() //Called form the takeoff animation event to renable the rigidbody based steering
    {
        _behaviorState = BehaviorState.Flying;
        _metaMaticTrajectory._rb.isKinematic = false;
        StartCoroutine(LerpPitchToLookAtTargetToValueBeforeLanding(.5f));
    }

    IEnumerator LerpPitchToLookAtTargetToValueBeforeLanding(float isSeconds)
    {
        float timer = 0f;
        while (timer < isSeconds)
        {
            timer += Time.deltaTime;
            _pitchToLookAtTarget =
                Mathf.Lerp(_pitchToLookAtTarget, _pitchToLookAtTargeBeforeLanding, timer / isSeconds);
            yield return null;
        }
    }

    private void PlayThinkingSfx()
    {
        if (!_sfxAudioSource) return;
        _sfxAudioSource.clip = _thinkingAudioClips[UnityEngine.Random.Range(0, _thinkingAudioClips.Length)];
        _sfxAudioSource.Play();
    }

    private void StopSfx()
    {
        if (!_sfxAudioSource) return;
        _sfxAudioSource.clip = null;
        _sfxAudioSource.Stop();
    }

    private void AudioSignalSimulation()
    {
        if (!_simulateMetaMaticAudioSignal)
        {
            return;
        }

        if (!_simulatingAudioSignal)
        {
            _emissiveSpeakingRenderer.material.SetVector("_EmissionColor", _defaultColor);
            _audioInput = 0;
        }
        else
        {
            _audioInput = Mathf.PerlinNoise1D(Time.time * 7f);
        }
    }

    public static Vector3 ToHSV(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return new Vector3(h, s, v);
    }
}
