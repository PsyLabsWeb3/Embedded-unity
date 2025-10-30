using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Quantum;
using UnityEngine.Serialization;

public unsafe class KartViewController : QuantumCallbacks
{
    [Header("Visual Kart parts")]
    [SerializeField] private Transform _visualRoot;

    [SerializeField] private Transform _steeringWheel;
    [SerializeField] private List<Transform> _frontWheels;
    [SerializeField] private List<Transform> _backWheels;

    [SerializeField] private float _aerialWheelOffset;

    [Header("Visual effects")]
    [SerializeField] private List<TrailRenderer> _driftTrails;

    [SerializeField] private List<ParticleSystem> _driftParticles;
    [SerializeField] private List<ParticleSystem> _boostParticles;

    [SerializeField] private ShieldEffectController _shield;

    [Header("Constant audio")]
    [SerializeField] private AudioSource _engineAudio;
    [SerializeField] private AudioSource _driftAudio;
    [SerializeField] private AudioSource _effectAudio;

    [Header("Audio clips")]
    [SerializeField] private AudioClip _boostSound;
    [SerializeField] private AudioClip _powerupSound;

    [Header("Visual configuration")]
    [SerializeField] private float _maxDriftingWheelAngle;

    [SerializeField] private float _maxSteeringWheelAngle;

    [SerializeField] private CinemachineVirtualCamera _raceCamera;
    [SerializeField] private CinemachineVirtualCamera _finishedCamera;

    private QuantumEntityView _entityView
    {
        get
        {
            if (_cachedView == null)
            {
                _cachedView = GetComponentInParent<QuantumEntityView>();
            }

            return _cachedView;
        }
    }

    private QuantumEntityView _cachedView = null;

    [SerializeField] private float _maxEnginePitch = 1.5f;
    [SerializeField] private float _minEnginePitch = 1.5f;

    private float _targetEnginePitchT = 1.0f;
    private float engineVel;

    public bool isAI => PlayerRef._index < 1;

    public CinemachineVirtualCamera RaceCamera => _raceCamera;
    public CinemachineVirtualCamera FinishedCamera => _finishedCamera;

    public int _AIIndex { get; private set; } = -1;

    public string DriverName;

    public QuantumEntityView EntityView => _entityView;

    public PlayerRef PlayerRef { get; private set; }

    private void Start()
    {
        QuantumEvent.Subscribe(this, (EventOnImmunityStart e) => OnImmunityStart(e));
        QuantumEvent.Subscribe(this, (EventOnImmunityEnd e) => OnImmunityEnd(e));
        QuantumEvent.Subscribe(this, (EventOnBoostStart e) => OnBoostStart(e));
        QuantumEvent.Subscribe(this, (EventWeaponCollected e) => OnWeaponCollected(e));
    }

    private void Update()
    {
        _engineAudio.pitch = Mathf.Lerp(_minEnginePitch, _maxEnginePitch, _targetEnginePitchT);
    }

    public override void OnUpdateView(QuantumGame game)
    {
        Frame frame = game.Frames.Predicted;
        if (!frame.Exists(_entityView.EntityRef))
        {
            return;
        }

        Kart* kartComp = frame.Unsafe.GetPointer<Kart>(_entityView.EntityRef);
        Drifting* driftingComp = frame.Unsafe.GetPointer<Drifting>(_entityView.EntityRef);
        KartInput* inputComp = frame.Unsafe.GetPointer<KartInput>(_entityView.EntityRef);
        KartBoost* boostComp = frame.Unsafe.GetPointer<KartBoost>(_entityView.EntityRef);

        _targetEnginePitchT = kartComp->GetNormalizedSpeed(frame).AsFloat;

        UpdateDrifting(driftingComp, kartComp);
        UpdateVisualWheels(driftingComp, inputComp, kartComp);
        UpdateSteeringWheel(inputComp);
        UpdateBoostEffects(boostComp);
    }

    public void Initialize(QuantumGame game)
    {
        Frame frame = game.Frames.Verified;

        if (frame.Unsafe.TryGetPointer(_entityView.EntityRef, out PlayerLink* playerLink))
        {
            PlayerRef = playerLink->Player;
        }
        else if (frame.Unsafe.TryGetPointer(_entityView.EntityRef, out AIDriver* ai))
        {
            _AIIndex = ai->AIIndex;
        }
        else
        {
            Debug.LogError("Neither player or AI driver?");
        }

        if (isAI)
        {
            AIDriver* ai = frame.Unsafe.GetPointer<AIDriver>(_entityView.EntityRef);
            DriverName = frame.FindAsset(ai->SettingsRef).Nickname;
        }
        else
        {
            DriverName = frame.GetPlayerData(PlayerRef).PlayerNickname;
        }

        PlayerManager.Instance.RegisterPlayer(game, this);
    }

    private void OnBoostStart(EventOnBoostStart callback)
    {
        if (callback.kartEntity != _entityView.EntityRef) { return; }

        _effectAudio.PlayOneShot(_boostSound);
    }

    private void OnWeaponCollected(EventWeaponCollected callback)
    {
        if (callback.kartEntity != _entityView.EntityRef || callback.kartEntity != LocalPlayerManager.Instance.LocalPlayerEntity) { return; }

        _effectAudio.PlayOneShot(_powerupSound, 0.5f);
    }

    private void OnImmunityEnd(EventOnImmunityEnd callback)
    {
        if (callback.kartEntity != _entityView.EntityRef) { return; }

        _shield.SetShieldVisible(false);
    }

    private void OnImmunityStart(EventOnImmunityStart callback)
    {
        if (callback.kartEntity != _entityView.EntityRef) { return; }

        _shield.SetShieldVisible(true);
    }

    public void OnEntityDestroyed(QuantumGame game)
    {
        PlayerManager.Instance.UnregisterPlayer(game, this);
    }

    private void UpdateSteeringWheel(KartInput* inputComp)
    {
        _steeringWheel.localRotation = Quaternion.Euler(0, 0, inputComp->GetTotalSteering().AsFloat * -45);
    }

    private void UpdateVisualWheels(Drifting* driftingComp, KartInput* inputComp, Kart* kartComp)
    {
        foreach (Transform wheel in _frontWheels)
        {
            wheel.localPosition = Vector3.MoveTowards(
                wheel.localPosition,
                kartComp->IsGrounded ? Vector3.zero : Vector3.up * _aerialWheelOffset,
                Time.deltaTime
            );

            float driftingOffset = driftingComp->Direction * -_maxDriftingWheelAngle;
            float steering = inputComp->GetTotalSteering().AsFloat * _maxSteeringWheelAngle;
            Quaternion r = wheel.localRotation;
            Quaternion t = Quaternion.Euler(0, Mathf.Clamp(driftingOffset + steering, -45, 45), 0);
            t *= Quaternion.Euler(Vector3.right * kartComp->Velocity.SqrMagnitude.AsFloat);
            wheel.localRotation = Quaternion.Slerp(r, t, Time.deltaTime * 10f);
        }

        foreach (var wheel in _backWheels)
        {
            wheel.localPosition = Vector3.MoveTowards(
                wheel.localPosition,
                kartComp->IsGrounded ? Vector3.zero : Vector3.up * _aerialWheelOffset,
                Time.deltaTime
            );
            Quaternion r = wheel.localRotation;
            Quaternion t = r * Quaternion.Euler(Vector3.right * kartComp->Velocity.SqrMagnitude.AsFloat);
            wheel.localRotation = Quaternion.Slerp(r, t, Time.deltaTime * 10f);
        }
    }

    private void UpdateBoostEffects(KartBoost* boostComp)
    {
        foreach (ParticleSystem particleSystem in _boostParticles)
        {
            bool boost = boostComp->CurrentBoost != null;

            if (!boost && particleSystem.isPlaying)
            {
                particleSystem.Stop();
            }
            else if (boost && !particleSystem.isPlaying)
            {
                particleSystem.Play();
            }
        }
    }

    private void UpdateDrifting(Drifting* driftingComp, Kart* kartComp)
    {
        Quaternion rot = _visualRoot.localRotation;
        Quaternion target = Quaternion.Euler(rot.eulerAngles.x, driftingComp->Direction * _maxDriftingWheelAngle, rot.eulerAngles.z);
        _visualRoot.localRotation = Quaternion.Slerp(rot, target, Time.deltaTime / .3f);

        bool driftingOnGround = driftingComp->Direction != 0 && kartComp->IsGrounded;

        if (_driftAudio.isPlaying && !driftingOnGround)
        {
            _driftAudio.Stop();
        }
        else if (!_driftAudio.isPlaying && driftingOnGround)
        {
            _driftAudio.Play();
        }

        foreach (TrailRenderer trail in _driftTrails)
        {
            trail.emitting = driftingOnGround;
        }

        foreach (ParticleSystem particleSystem in _driftParticles)
        {
            if (particleSystem.isPlaying && !driftingOnGround)
            {
                particleSystem.Stop();
            }
            else if (!particleSystem.isPlaying && driftingOnGround)
            {
                particleSystem.Play();
            }
        }
    }
}
