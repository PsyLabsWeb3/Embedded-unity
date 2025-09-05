using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class UIAnimation : MonoBehaviour
{
    [SerializeField] private bool _auto;
    [SerializeField] private bool _loop;
    [SerializeField] private Animation _animation;

    private void OnEnable()
    {
        _animation.wrapMode = _loop ? WrapMode.Loop : WrapMode.Once;

        if (_auto)
        {
            _animation.Play();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_animation == null)
        {
            _animation = GetComponent<Animation>();
        }
    }
#endif

    public void PlayOnce()
    {
        _animation.wrapMode = WrapMode.Once;
        _animation.Play();
    }
}
