using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldEffectController : MonoBehaviour
{
    [SerializeField] private GameObject _effectObject;
    [SerializeField] private float _maxStrength = 0.6f;
    [SerializeField] private float _animTime = 0.15f;

    private Material _material;
    private Vector3 _fullScale;

    private void Awake()
    {
        _material = _effectObject.GetComponent<MeshRenderer>().material;
        _fullScale = _effectObject.transform.localScale;
        _effectObject.SetActive(false);
    }

    public void SetShieldVisible(bool visible)
    {
        if (visible) { _effectObject.SetActive(true); }

        StartCoroutine(Animate(visible));
    }

    IEnumerator Animate(bool visible)
    {
        float targetStrength = visible ? _maxStrength : 0f;
        float initStrength = visible ? 0f : _maxStrength;
        Vector3 targetScale = visible ? _fullScale : Vector3.zero;
        Vector3 initScale = visible ? Vector3.zero : _fullScale;

        float t = 0f;

        while (t < 1.0f)
        {
            t = Mathf.Min(t + Time.deltaTime / _animTime, 1f);

            _material.SetFloat("_Strength", Mathf.Lerp(initStrength, targetStrength, t));
            _effectObject.transform.localScale = Vector3.Lerp(initScale, targetScale, t);

            yield return null;
        }

        if (!visible)
        {
            _effectObject.gameObject.SetActive(false);
        }
    }
}
