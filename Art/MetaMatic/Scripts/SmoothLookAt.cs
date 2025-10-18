using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothLookAt : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] float _damp = 1f;
    void Start()
    {

    }

    void Update()
    {
        Vector3 dir = _target.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * _damp);
    }
}
