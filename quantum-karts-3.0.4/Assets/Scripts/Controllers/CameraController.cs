using System;
using System.Collections;
using UnityEngine;
using Quantum;
using Cinemachine;

public unsafe class CameraController : MonoSingleton<CameraController>
{

    [Header("Preview settings")]
    [SerializeField] private Vector3 _previewCenter;

    [SerializeField] private Vector3 _previewOffset;
    [SerializeField] private float _previewSpeed;

    private bool _previewCam = true;

    [SerializeField] private Transform _camTransform;
    [SerializeField] private CinemachineBrain _brain;

    private Coroutine _previewCamRoutine;

    private void Start()
    {
        Frame frame = QuantumRunner.Default?.Game?.Frames?.Verified;
        if (frame != null && frame.Unsafe.TryGetPointerSingleton(out Race* race))
        {
            HandleRaceState(race->CurrentRaceState);
        }
        else
        {
            HandleRaceState(RaceState.Waiting);
        }

        QuantumEvent.Subscribe(this, (EventOnRaceStateChanged e) => OnRaceStateChanged(e));
        QuantumEvent.Subscribe(this, (EventOnPlayerFinished e) => OnPlayerFinished(e));
    }

    private void OnPlayerFinished(EventOnPlayerFinished data)
    {
        if (data.entityRef == LocalPlayerManager.Instance.LocalPlayerEntity)
        {
            LocalPlayerManager.Instance.LocalPlayerKartView.FinishedCamera.enabled = true;
            LocalPlayerManager.Instance.LocalPlayerKartView.RaceCamera.enabled = false;
        }
    }

    private void OnRaceStateChanged(EventOnRaceStateChanged data)
    {
        HandleRaceState(data.state);
    }

    private void HandleRaceState(RaceState state)
    {
        if (_previewCam && state != RaceState.None && state != RaceState.Waiting)
        {
            EndPreviewCamera();
        }

        switch (state)
        {
            case RaceState.Countdown:
            {
                LocalPlayerManager.Instance.LocalPlayerKartView.FinishedCamera.enabled = false;
                LocalPlayerManager.Instance.LocalPlayerKartView.RaceCamera.enabled = true;
                break;
            }
            case RaceState.Waiting:
            {
                StartPreviewCamera();
                break;
            }
            case RaceState.Finished:
            {
                LocalPlayerManager.Instance.LocalPlayerKartView.FinishedCamera.enabled = true;
                LocalPlayerManager.Instance.LocalPlayerKartView.RaceCamera.enabled = false;
                // show front view
                break;
            }
            default: break;
        }
    }

    private void StartPreviewCamera()
    {
        _previewCamRoutine = StartCoroutine(PreviewCamera_Coroutine());
        _brain.enabled = false;
        _previewCam = true;
    }

    private void EndPreviewCamera()
    {
        StopCoroutine(_previewCamRoutine);
        _brain.enabled = true;
        _previewCam = false;
    }

    private IEnumerator PreviewCamera_Coroutine()
    {
        while (_previewCam)
        {
            _camTransform.position = _previewCenter +
                                     Quaternion.AngleAxis(Time.time * _previewSpeed, Vector3.up) *
                                     _previewOffset;
            _camTransform.rotation = Quaternion.LookRotation(_previewCenter - _camTransform.position, Vector3.up);
            yield return null;
        }
    }
}