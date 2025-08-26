namespace PlatformShooter2D
{
  using System.Collections.Generic;
  using Quantum;
  using UnityEngine;

  /// <summary>
  /// This Behavior handles events related to character actions that require audio feedback, such as Jump, Double Jumping, Death and Respawn
  /// 
  /// Uses the default Unity Audio API for simplicity
  /// </summary>
  public class CharacterAudioController : QuantumEntityViewComponent
  {
    [Header("References")] public AudioListener AudioListener;
    public AudioSource AudioSourcePrefab;

    [Header("Configurations")] public int MaxAudioSources = 8;
    public Transform AudioSourceParent;

    [Header("Audio Refs")] public AudioConfiguration JumpAudioClip;
    public AudioConfiguration DoubleJumpAudioClip;
    public AudioConfiguration LandingAudioClip;
    public AudioConfiguration DeathClip;
    public AudioConfiguration RespawnClip;
    public AudioConfiguration FootstepsClip;

    private readonly Stack<AudioSource> _freeAudioSources = new Stack<AudioSource>();
    private List<AudioSource> _audioSourcesInUse = new List<AudioSource>();

    public override void OnActivate(Frame frame)
    {
      base.OnActivate(frame);
      for (int i = 0; i < MaxAudioSources; i++)
      {
        var audioSource = Instantiate(AudioSourcePrefab, AudioSourceParent);
        audioSource.transform.localPosition = Vector3.zero;

        _freeAudioSources.Push(audioSource);
      }

      // Register for relevant events		
      QuantumEvent.Subscribe<EventJumped>(this, OnJump);
      QuantumEvent.Subscribe<EventLanded>(this, OnLanding);
      QuantumEvent.Subscribe<EventOnCharacterDeath>(this, OnDeath);
      QuantumEvent.Subscribe<EventOnCharacterRespawn>(this, OnRespawn);
      CheckLocalAudioListener();
    }

    private void CheckLocalAudioListener()
    {
      if (VerifiedFrame.Exists(EntityRef) == false)
      {
        return;
      }

      var player = VerifiedFrame.Get<PlayerLink>(EntityRef);
      if (Game.PlayerIsLocal(player.Player))
      {
        AudioListener.enabled = true;
        var al = Camera.main.GetComponent<AudioListener>();
        Destroy(al);
      }
    }

    void Update()
    {
      for (var i = _audioSourcesInUse.Count - 1; i >= 0; i--)
      {
        var source = _audioSourcesInUse[i];
        if (!source.isPlaying)
        {
          _freeAudioSources.Push(source);
          _audioSourcesInUse.RemoveAt(i);
        }
      }
    }

    AudioSource GetAvailableAudioSource()
    {
      if (_freeAudioSources.Count > 0)
      {
        var source = _freeAudioSources.Pop();
        _audioSourcesInUse.Add(source);
        return source;
      }
      else
      {
        var source = _audioSourcesInUse[0];
        _audioSourcesInUse.RemoveAt(0);
        _audioSourcesInUse.Add(source);
        return source;
      }
    }

    void PlayAudioClip(AudioConfiguration audioConfig)
    {
      var source = GetAvailableAudioSource();
      audioConfig.AssignToAudioSource(source);
      source.Play();
    }

    private void OnJump(EventJumped eventData)
    {
      if (EntityRef.Equals(eventData.Entity))
      {
        var kcc = PredictedFrame.Get<KCC2D>(eventData.Entity);
        if(kcc._state == KCCState.JUMPED) PlayAudioClip(JumpAudioClip);;
        if(kcc._state == KCCState.DOUBLE_JUMPED) PlayAudioClip(DoubleJumpAudioClip);; 
      }
    }

    private void OnLanding(EventLanded eventData)
    {
      if (EntityRef.Equals(eventData.Entity))
        PlayAudioClip(LandingAudioClip);
    }

    private void OnDeath(EventOnCharacterDeath eventData)
    {
      if (EntityRef.Equals(eventData.Character))
        PlayAudioClip(DeathClip);
    }

    private void OnRespawn(EventOnCharacterRespawn eventData)
    {
      if (EntityRef.Equals(eventData.Character))
      {
        PlayAudioClip(RespawnClip);
      }
    }

    // Triggered via animation events
    public void OnFootStep()
    {
      PlayAudioClip(FootstepsClip);
    }
  }
}