// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// Centralised sound manager for MetaMatic.
/// Attach this component to the robot GameObject (same object as MetaMaticAnimController)
/// so spatialization originates from the robot's transform.
/// </summary>
public class AudioController : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource _oneShot; // all SFX cues (overlapping allowed)
    [SerializeField] private AudioSource _loop; // loops (flying/thinking)
    [SerializeField] private AudioSource _plantPlacement;
    [SerializeField] private AudioSource noticingPlayer;

    [Header("One-Shot Cues")]
    [SerializeField] private List<AudioClip> _micStartClips = new();
    [SerializeField] private List<AudioClip> _micStopClips = new();
    [SerializeField] private List<AudioClip> _noticingClips = new();
    [SerializeField] private List<AudioClip> _detectionClips = new();
    [SerializeField] private AudioClip _plantPlacementClip;
    [SerializeField] private AudioClip _landingClip;

    [Header("Loops")]
    [SerializeField] private AudioClip _flyingLoop;
    [SerializeField] private AudioClip _thinkingLoop;

    [Header("Debug")]
    [SerializeField] private bool _verboseLogs = false;

    private void OnEnable()
    {
        // Harden defaults so short cues aren't culled or spatially odd.
        if (_oneShot != null)
        {
            _oneShot.spatialBlend = 1f; // 3D
            _oneShot.priority = 64; // higher than default
            _oneShot.reverbZoneMix = 0f;
        }

        if (_loop != null)
        {
            _loop.spatialBlend = 1f;
            _loop.priority = 80;
            _loop.loop = true;
            _loop.reverbZoneMix = 0f;
        }
    }

    public void PlayMicStart() => PlayOneShot(GetRandom(_micStartClips), "MicStart");
    public void PlayMicStop() => PlayOneShot(GetRandom(_micStopClips), "MicStop");
    public void PlayNoticing()
    {
        noticingPlayer.PlayOneShot(GetRandom(_noticingClips));
    }

    public void PlayDetection() => PlayOneShot(GetRandom(_detectionClips), "Detection");
    public void PlayPlantPlacement()
    {
        _plantPlacement.PlayOneShot(_plantPlacementClip);
    }

    public void PlayLanding() => PlayOneShot(_landingClip, "Landing");

    public void StartFlyingLoop() => PlayLoop(_flyingLoop, "FlyingLoop");
    public void StopFlyingLoop() => StopLoop(_flyingLoop);
    public void StartThinkingLoop() => PlayLoop(_thinkingLoop, "ThinkingLoop");
    public void StopThinkingLoop() => StopLoop(_thinkingLoop);

    private void PlayOneShot(AudioClip clip, string tag = null)
    {
        if (_oneShot == null)
        {
            Debug.LogWarning("[AudioController] _oneShot AudioSource is missing.", this);
            return;
        }

        if (clip == null)
        {
            if (_verboseLogs) Debug.Log($"[AudioController] OneShot skipped (null clip) tag={tag}", this);
            return;
        }

        // IMPORTANT: allow overlap; never Stop() the one-shot channel.
        _oneShot.PlayOneShot(clip);

        if (_verboseLogs)
            Debug.Log($"[AudioController] OneShot tag={tag} clip='{clip.name}' vol={_oneShot.volume}", this);
    }

    private void PlayLoop(AudioClip clip, string tag = null)
    {
        if (_loop == null)
        {
            Debug.LogWarning("[AudioController] _loop AudioSource is missing.", this);
            return;
        }

        if (!clip) return;

        // Only (re)start if different to avoid clicks
        if (_loop.isPlaying && _loop.clip == clip)
        {
            if (_verboseLogs) Debug.Log($"[AudioController] Loop already playing tag={tag} clip='{clip.name}'", this);
            return;
        }

        _loop.Stop();
        _loop.clip = clip;
        _loop.Play();

        if (_verboseLogs) Debug.Log($"[AudioController] Loop start tag={tag} clip='{clip.name}'", this);
    }

    private void StopLoop(AudioClip clip)
    {
        if (_loop == null || !clip) return;
        if (_loop.isPlaying && _loop.clip == clip)
        {
            _loop.Stop();
            if (_verboseLogs) Debug.Log($"[AudioController] Loop stop clip='{clip.name}'", this);
        }
    }

    private static AudioClip GetRandom(List<AudioClip> list)
        => list == null || list.Count == 0 ? null : list[Random.Range(0, list.Count)];

#if UNITY_EDITOR
    [ContextMenu("Test Noticing")]
    private void __TestNoticing() => PlayNoticing();

    [ContextMenu("Test Plant Placement")]
    private void __TestPlantPlacement() => PlayPlantPlacement();

    [ContextMenu("Test Detection")]
    private void __TestDetection() => PlayDetection();

    [ContextMenu("Test Landing")]
    private void __TestLanding() => PlayLanding();

    [ContextMenu("Test Flying Loop")]
    private void __TestFlyingLoop() => StartFlyingLoop();

    [ContextMenu("Stop Flying Loop")]
    private void __StopFlyingLoop() => StopFlyingLoop();

    [ContextMenu("Test Thinking Loop")]
    private void __TestThinkingLoop() => StartThinkingLoop();

    [ContextMenu("Stop Thinking Loop")]
    private void __StopThinkingLoop() => StopThinkingLoop();
#endif
}
