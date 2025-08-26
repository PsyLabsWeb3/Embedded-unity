namespace PlatformShooter2D
{
  using UnityEngine;

  /// <summary>
  /// Represents the configuration settings for an audio clip, 
  /// including its properties and methods to manage playback.
  /// </summary>
  [System.Serializable]
  public struct AudioConfiguration
  {
    /// <summary>
    /// The audio clip to be played.
    /// </summary>
    public AudioClip Clip;

    /// <summary>
    /// The playback volume of the audio clip, ranging from 0 (mute) to 1.0 (full volume).
    /// </summary>
    [Range(0, 1.0f)] public float Volume;

    /// <summary>
    /// Determines whether the audio should be played as 2D (spatialBlend = 0) or 3D (spatialBlend = 1).
    /// </summary>
    public bool Is2D;

    /// <summary>
    /// Specifies whether the audio clip should loop when it reaches the end.
    /// </summary>
    public bool Loop;

    /// <summary>
    /// The delay before the audio starts playing, in seconds.
    /// </summary>
    public float Delay;

    /// <summary>
    /// Returns the name of the audio clip or a default message if no clip is selected.
    /// </summary>
    public string Name
    {
      get { return Clip == null ? "No Clip selected" : Clip.name; }
    }

    /// <summary>
    /// Checks if the configuration is valid, which requires the presence of an audio clip.
    /// </summary>
    /// <returns>True if the Clip is not null; otherwise, false.</returns>
    public bool IsValid()
    {
      return Clip != null;
    }

    /// <summary>
    /// Assigns the current configuration settings to a given AudioSource component.
    /// </summary>
    /// <param name="audioSource">The AudioSource to configure.</param>
    public void AssignToAudioSource(AudioSource audioSource)
    {
      audioSource.volume = Volume;
      audioSource.clip = Clip;
      audioSource.spatialBlend = Is2D ? 0.0f : 1.0f;
      audioSource.loop = Loop;
    }
  }
}