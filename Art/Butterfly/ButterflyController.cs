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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Meta.XR.SharedAssets
{
    public class ButterflyController : MonoBehaviour
    {
        [SerializeField] GameObject _leftWing;
        [SerializeField] GameObject _rightWing;
        [SerializeField] float _flapAmplitude = 1;
        [SerializeField] float _flapSpeed = 10;
        private float _flappingOffset;
        private float _flapSpeedRandomization;
        [SerializeField] float _flyingSpeed = 10;
        [SerializeField] float _flightDisturbance = 1;
        [Tooltip("The butterfly will randomly pick a transform for its next goal until it gets close to it, then it will pick randomly the next one.")]
        public Transform[] _checkpoints;
        private int _checkpointsN = 0;
        private Rigidbody _rb;
        private Transform _target;
        [SerializeField] bool _randomCheckpointOrder = true;
        ButterflyController[] otherButterflies;
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _target = new GameObject("Target").transform;
            _flappingOffset = Random.Range(0, 100);
            _flapSpeedRandomization = Random.Range(-5, 5);
            otherButterflies = FindObjectsOfType<ButterflyController>();
            if (_randomCheckpointOrder)
            {
                _checkpointsN = Random.Range(0, _checkpoints.Length);
            }

        }

        void Update()
        {
            //Wings flapping
            float sin = Mathf.Sin((Time.time + _flappingOffset) * (_flapSpeed + _flapSpeedRandomization));
            _leftWing.transform.localEulerAngles = new Vector3(0, 0, (sin * _flapAmplitude) - 90);
            _rightWing.transform.localEulerAngles = new Vector3(0, 180, (sin * _flapAmplitude) - 90);

            //Trajectory
            _target.Translate((_checkpoints[_checkpointsN].position - _target.position).normalized * Time.deltaTime * _flyingSpeed);
            //Get another random checkpoint goal or loops to chase the next checkpoints
            float distance = Vector3.Distance(_checkpoints[_checkpointsN].position, _target.position);
            if (distance < .01f)
            {
                _checkpointsN++;
                if (_randomCheckpointOrder)
                {
                    _checkpointsN = Random.Range(0, _checkpoints.Length);
                }
                else
                {
                    _checkpointsN = 0;
                }
            }
            //Orientation
            transform.rotation = Quaternion.LookRotation(Vector3.up, _target.position - transform.position);
        }

        void FixedUpdate()
        {
            Vector3 perlinNoise3d = new Vector3(Mathf.PerlinNoise(Time.time, 0) - .5f, Mathf.PerlinNoise(Time.time, 10) - .5f, Mathf.PerlinNoise(Time.time, 20) - .5f) * _flightDisturbance;
            Vector3 disturbedTarget = _target.position + perlinNoise3d;
            _rb.AddForce(disturbedTarget - this.transform.position);
            //avoid getting to close to another butterfly
            foreach (var b in otherButterflies)
            {
                if (b == this)
                {
                    continue;
                }
                float sqrMag = Vector3.SqrMagnitude(this.transform.position - b.transform.position);
                _rb.AddForce((this.transform.position - b.transform.position) * .1f / sqrMag);
            }
        }
    }
}
