using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    [SerializeField] private float _posChange = 0.5f;
    [SerializeField] private float _rotChange = 0.5f;
    [SerializeField] private float _speed = 1.0f;

    private Vector3 _originalPos;

    // Start is called before the first frame update
    void Awake()
    {
        _originalPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = _originalPos + (Vector3.up * (Mathf.Sin(Time.time * _speed) * _posChange));
        transform.Rotate(_rotChange * _speed, _rotChange * _speed, _rotChange * _speed);
    }
}