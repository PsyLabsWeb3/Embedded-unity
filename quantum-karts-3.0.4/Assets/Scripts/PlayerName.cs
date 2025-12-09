using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class PlayerName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private AnimationCurve _alphaCurve;
    private Transform _camera;

    private void Awake()
    {
        _camera = Camera.main.transform;
    }

    public void SetName(string name)
    {
        _text.text = name;
    }

    private void Update()
    {
        transform.localPosition = Vector3.up * 2;
        transform.rotation = Quaternion.LookRotation(transform.position - _camera.position, Vector3.up);
        _group.alpha = _alphaCurve.Evaluate(Vector3.Distance(_camera.position, transform.position));
    }
}