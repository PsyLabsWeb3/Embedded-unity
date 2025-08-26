using UnityEngine.Serialization;

namespace PlatformShooter2D
{
  using System.Collections.Generic;
  using Quantum;
  using UnityEngine;

  // ReSharper disable InvalidXmlDocComment
  /// <summary>
  /// SfxController handles events not related to player actions that require audio feedback, such as shooting and explosions.
  /// Uses the default Unity Audio API.
  /// </summary>
  public class SfxController : QuantumSceneViewComponent<CustomViewContext>
  {
    [Header("References")]
    /// <summary>
    /// The prefab for the audio source to be used for playing sound effects.
    /// </summary>
    public AudioSource AudioSourcePrefab;

    [Header("Configurations")]
    /// <summary>
    /// The maximum number of audio sources allowed.
    /// </summary>
    public int MaxAudioSources = 16;

    /// <summary>
    /// The default parent transform for audio sources.
    /// </summary>
    public Transform AudioSourceDefaultParent;

    [Header("Audios")]
    /// <summary>
    /// The audio configuration for when a player is hit.
    /// </summary>
    public AudioConfiguration CharacterHitAudio;

    /// <summary>
    /// The audio configuration for when a player kills another player.
    /// </summary>
    public AudioConfiguration CharacterKillAudio;

    /// <summary>
    /// The audio configuration for when a player takes damage.
    /// </summary>
    public AudioConfiguration CharacterDamageTakenAudio;

    /// <summary>
    /// The audio configuration for when a skill is being cast.
    /// </summary>
    public AudioConfiguration SkillCastingAudio;

    /// <summary>
    /// The audio configuration for when a skill is activated.
    /// </summary>
    public AudioConfiguration SkillActivationAudio;

    /// <summary>
    /// The updater used to get views of entities.
    /// </summary>
    public QuantumEntityViewUpdater ViewUpdater;

    /// <summary>
    /// Stack of free audio sources to be reused.
    /// </summary>
    private readonly Stack<AudioSource> _freeAudioSources = new Stack<AudioSource>();

    /// <summary>
    /// List of audio sources currently in use.
    /// </summary>
    private List<AudioSource> _audioSourcesInUse = new List<AudioSource>();


    /// <summary>
    /// Initializes the audio sources and subscribes to relevant events.
    /// </summary>
    private void Start()
    {
      // Instantiate the required number of audio sources and add them to the free stack.
      for (int i = 0; i < MaxAudioSources; i++)
      {
        var audioSource = Instantiate(AudioSourcePrefab, AudioSourceDefaultParent);
        audioSource.transform.localPosition = Vector3.zero;

        _freeAudioSources.Push(audioSource);
      }

      // Subscribe to events that require audio feedback.
      QuantumEvent.Subscribe<EventOnCharacterTakeDamage>(this, OnCharacterDamage);
      QuantumEvent.Subscribe<EventOnWeaponShoot>(this, OnWeaponShot);
      QuantumEvent.Subscribe<EventOnBulletDestroyed>(this, OnBulletDestroyed);
      QuantumEvent.Subscribe<EventOnSkillCasted>(this, OnSkillCasted);
      QuantumEvent.Subscribe<EventOnSkillActivated>(this, OnSkillActivated);
      QuantumEvent.Subscribe<EventOnCharacterDeath>(this, OnCharacterDeath);
    }

    /// <summary>
    /// Updates the status of the audio sources, recycling unused sources.
    /// </summary>
    private void Update()
    {
      // Check if any audio sources are no longer playing, and recycle them.
      for (var i = _audioSourcesInUse.Count - 1; i >= 0; i--)
      {
        var source = _audioSourcesInUse[i];
        if (!source.isPlaying)
        {
          _freeAudioSources.Push(source);
          _audioSourcesInUse.RemoveAt(i);
          source.transform.SetParent(AudioSourceDefaultParent);
          source.transform.position = Vector3.zero;
        }
      }
    }

    /// <summary>
    /// Gets an available audio source from the pool.
    /// </summary>
    /// <returns>An available AudioSource.</returns>
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

    /// <summary>
    /// Plays an audio clip using the provided audio configuration.
    /// </summary>
    /// <param name="audioConfig">The audio configuration to use.</param>
    void PlayAudioClip(AudioConfiguration audioConfig)
    {
      var source = GetAvailableAudioSource();
      audioConfig.AssignToAudioSource(source);

      source.transform.position = Vector3.zero;
      source.Play();
    }

    /// <summary>
    /// Plays an audio clip using the provided audio configuration and parent transform.
    /// </summary>
    /// <param name="audioConfig">The audio configuration to use.</param>
    /// <param name="parent">The parent transform for the audio source.</param>
    void PlayAudioClip(AudioConfiguration audioConfig, Transform parent)
    {
      var source = GetAvailableAudioSource();
      audioConfig.AssignToAudioSource(source);

      source.transform.SetParent(parent);
      source.transform.localPosition = Vector3.zero;
      source.Play();
    }

    /// <summary>
    /// Plays an audio clip using the provided audio configuration and world position.
    /// </summary>
    /// <param name="audioConfig">The audio configuration to use.</param>
    /// <param name="position">The world position for the audio source.</param>
    private void PlayAudioClip(AudioConfiguration audioConfig, Vector3 position)
    {
      var source = GetAvailableAudioSource();
      audioConfig.AssignToAudioSource(source);

      source.transform.position = position;
      source.Play();
    }

    /// <summary>
    /// Handles the event when a character takes damage and plays the appropriate sound.
    /// </summary>
    /// <param name="eventData">The event data containing information about the damage event.</param>
    private void OnCharacterDamage(EventOnCharacterTakeDamage eventData)
    {
      var targetCharacterTransform = PredictedFrame.Get<Transform2D>(eventData.Character);
      var audioConfig = eventData.Character == ViewContext.LocalCharacterView.EntityRef
        ? CharacterDamageTakenAudio
        : CharacterHitAudio;
      PlayAudioClip(audioConfig, targetCharacterTransform.Position.ToUnityVector3());
    }

    /// <summary>
    /// Handles the event when a weapon is fired and plays the shooting sound.
    /// </summary>
    /// <param name="eventData">The event data containing information about the weapon shoot event.</param>
    private void OnWeaponShot(EventOnWeaponShoot eventData)
    {
      var weaponInventory = VerifiedFrame.Get<WeaponInventory>(eventData.Character);
      var weapon = weaponInventory.Weapons[weaponInventory.CurrentWeaponIndex];
      var weaponData = VerifiedFrame.FindAsset(weapon.WeaponData);

      var characterView = ViewUpdater.GetView(eventData.Character);

      var characterTransform = VerifiedFrame.Get<Transform2D>(eventData.Character);

      if (characterView != null)
      {
        PlayAudioClip(weaponData.ShootAudioInfo, characterView.transform);
      }
      else
      {
        PlayAudioClip(weaponData.ShootAudioInfo, characterTransform.Position.ToUnityVector3());
      }
    }

    /// <summary>
    /// Handles the event when a bullet is destroyed and plays the destruction sound.
    /// </summary>
    /// <param name="eventData">The event data containing information about the bullet destruction event.</param>
    private void OnBulletDestroyed(EventOnBulletDestroyed eventData)
    {
      var asset = QuantumUnityDB.GetGlobalAsset<BulletData>(eventData.BulletData);
      PlayAudioClip(asset.BulletDestroyAudioInfo, eventData.BulletPosition.ToUnityVector3());
    }

    /// <summary>
    /// Handles the event when a skill is cast and plays the casting sound.
    /// </summary>
    /// <param name="eventData">The event data containing information about the skill casting event.</param>
    private void OnSkillCasted(EventOnSkillCasted eventData)
    {
      if (PredictedFrame.Exists(eventData.Skill) == false)
      {
        return;
      }

      var skillFields = PredictedFrame.Get<SkillFields>(eventData.Skill);
      var skillTransform = PredictedFrame.Get<Transform2D>(eventData.Skill);
      var characterView = ViewUpdater.GetView(skillFields.Source);

      if (characterView != null)
      {
        PlayAudioClip(SkillCastingAudio, characterView.transform);
      }
      else
      {
        PlayAudioClip(SkillCastingAudio, skillTransform.Position.ToUnityVector3());
      }
    }

    /// <summary>
    /// Handles the event when a skill is activated and plays the activation sound.
    /// </summary>
    /// <param name="eventData">The event data containing information about the skill activation event.</param>
    private void OnSkillActivated(EventOnSkillActivated eventData)
    {
      PlayAudioClip(SkillActivationAudio, eventData.SkillPosition.ToUnityVector3());
    }

    /// <summary>
    /// Handles the event when a character dies and plays the appropriate sound for the killer.
    /// </summary>
    /// <param name="eventData">The event data containing information about the character death event.</param>
    private void OnCharacterDeath(EventOnCharacterDeath eventData)
    {
      var player = PredictedFrame.Get<PlayerLink>(eventData.Killer);
      if (Game.PlayerIsLocal(player.Player))
      {
        PlayAudioClip(CharacterKillAudio);
      }
    }
  }
}