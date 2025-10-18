/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MetaMaticTrajectory : MonoBehaviour
{
    [SerializeField] float _rigidBodyMass = 1;
    [SerializeField] float _rigidBodyDrag = 1;
    [SerializeField] float _force = 1;
    [SerializeField] public float _floatAmplitude = .5f;
    [SerializeField] float _floatFrequency = .5f;
    [SerializeField] float _straightness = 20f;
    [SerializeField] float _rotationHitRecovery = 1f;

    [SerializeField] public Transform _goal;
    [SerializeField] public Transform _goalLookAt;
    [SerializeField] float _lookAtSmoothness = .5f;
    [Tooltip("Add transforms here to use them as checkpoints, the goal position and lookat will be driven by their position and forward vectors.")]
    [SerializeField] List<Transform> _checkpoints = new List<Transform>();
    [SerializeField] bool _keyboardControl = false;
    public float _intervalTime = 3;
    private int _targetCheckPointN = 0;
    private int _nextCheckPointN = 1;
    private float _timer = 0;
    private bool _followCheckpoitns = false;
    [HideInInspector] public Rigidbody _rb;
    private Vector3 _floatyPos;
    private Vector3 _upAim;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;

        if (_checkpoints.Count > 1)
        {
            _followCheckpoitns = true;
            _goalLookAt.SetParent(_goal);
            _goalLookAt.transform.localPosition = Vector3.forward * 2;
            _goal.position = Vector3.Lerp(this.transform.position, _checkpoints[_targetCheckPointN].position, .5f);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _goal.rotation, _lookAtSmoothness * Time.deltaTime);
        }
    }
    void Update()
    {
        _rb.mass = _rigidBodyMass;
        _rb.linearDamping = _rigidBodyDrag;
        _upAim = _floatyPos + Vector3.up * _straightness;
        if (_upAim.y < transform.position.y)
        {
            _upAim.y = transform.position.y + .2f;
        }
        if (_followCheckpoitns)
        {
            _timer += Time.deltaTime;
            _goal.position = Vector3.Lerp(_checkpoints[_targetCheckPointN].position, _checkpoints[_nextCheckPointN].position, _timer / _intervalTime);
            if (_timer >= _intervalTime)
            {
                _timer = 0;
                //If it's not the case to restart the looping trajectory...
                if (_nextCheckPointN == 0)
                {
                    _targetCheckPointN = 0;
                    _nextCheckPointN = 1;
                }
                //...increment the checkpoints...
                else
                {
                    _targetCheckPointN++;
                    _nextCheckPointN = _targetCheckPointN + 1;
                    //...untill you reach the last one when we have to aim to the starting point.
                    if (_nextCheckPointN == _checkpoints.Count)
                    {
                        _targetCheckPointN = _checkpoints.Count - 1;
                        _nextCheckPointN = 0;
                    }
                }
            }
            Vector3 lerpedAim = Vector3.Lerp(Vector3.ProjectOnPlane(_checkpoints[_targetCheckPointN].forward, Vector3.up), Vector3.ProjectOnPlane(_checkpoints[_nextCheckPointN].forward, Vector3.up), _timer / _intervalTime);
            Quaternion lookAt = Quaternion.LookRotation(_upAim - this.transform.position, lerpedAim);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookAt, _lookAtSmoothness * Time.deltaTime);
        }
        else
        {
            Quaternion lookAt = Quaternion.LookRotation(_upAim - this.transform.position, _goalLookAt.position - this.transform.position);
            _rb.MoveRotation(Quaternion.Lerp(this.transform.rotation, lookAt, _lookAtSmoothness * Time.deltaTime));
        }
        if (_rb.isKinematic == false)
        {
            _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, _rotationHitRecovery * Time.deltaTime);
        }
    }

    public IEnumerator BringFloatAmplitudeTo(float toValue, float inSeconds)
    {
        float timer = 0;
        while (timer < inSeconds)
        {
            timer += Time.deltaTime;
            _floatAmplitude = Mathf.Lerp(_floatAmplitude, toValue, timer / inSeconds);
            yield return null;
        }
    }

    void FixedUpdate()
    {
        if (_keyboardControl)
        {
            _goal.position = transform.position;
            _goal.position += transform.up * Input.GetAxis("Vertical");
            _goal.position += transform.right * Input.GetAxis("Horizontal");
        }
        _floatyPos = _goal.position + new Vector3(Mathf.PerlinNoise(Time.time * _floatFrequency, 0) - .5f, Mathf.PerlinNoise(Time.time * _floatFrequency, 1) - .5f, Mathf.PerlinNoise(Time.time * _floatFrequency, 2) - .5f) * _floatAmplitude;
        float chasingForce = Vector3.Distance(this.transform.position, _goal.position);
        Vector3 direction = (_floatyPos - this.transform.position).normalized;
        if (!_followCheckpoitns)
        {
            _rb.AddForce(direction * chasingForce * _force);
        }
        else
        {
            _rb.AddForce(direction * chasingForce * _force);
            Vector3 lerpedAim = Vector3.Lerp(_checkpoints[_targetCheckPointN].forward, _checkpoints[_nextCheckPointN].forward, _timer / _intervalTime);
        }

    }
}
