using UnityEngine;
[RequireComponent(typeof(Animator))]
public class AnimationSpeedRandomizer : MonoBehaviour
{
    private float _t = 0;
    Animator _animator;
    [SerializeField] AnimationCurve[] _curves;
    [SerializeField] KeyCode _previewKey = KeyCode.None;
    int _pickedCurveIndex = 0;
    float _animationDuration = 1;
    void Start()
    {
        if (_curves.Length == 0)
        {
            Debug.LogError("Create a few animation curves. The animation pick one randomly and play based on that.");
        }
        _animator = GetComponent<Animator>();
        _pickedCurveIndex = Random.Range(0, _curves.Length);
        _animationDuration = _animator.GetCurrentAnimatorStateInfo(0).length;
    }
    void LateUpdate()
    {
        if (_curves.Length == 0)
        {
            //Create a few animation curves. The animation pick one randomly and play based on that.
            return;
        }
        _t += Time.deltaTime;
        _animator.Play(0, 0, _curves[_pickedCurveIndex].Evaluate(_t * _animationDuration));
        if (_previewKey != KeyCode.None && Input.GetKeyDown(_previewKey))
        {
            _pickedCurveIndex = Random.Range(0, _curves.Length);
            _t = 0;
        }
    }
}
