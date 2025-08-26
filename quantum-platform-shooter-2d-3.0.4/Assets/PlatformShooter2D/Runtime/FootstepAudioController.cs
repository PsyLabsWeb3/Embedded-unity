namespace PlatformShooter2D
{
  using UnityEngine;
  using Quantum;

  /// <summary>
  /// Handles the audio playback for character footsteps.
  /// Determines when to play footstep sounds based on the entity's movement state and velocity.
  /// </summary>
  public class FootstepAudioController : QuantumEntityViewComponent
  {
    /// <summary>
    /// Reference to the audio controller responsible for playing footstep sounds.
    /// </summary>
    public CharacterAudioController AudioController;

    /// <summary>
    /// Delay between consecutive footstep sounds.
    /// </summary>
    public float StepsDelay;

    /// <summary>
    /// Minimum velocity threshold to trigger footstep sounds.
    /// </summary>
    public float VelocityThreshold = 0.5f;

    /// <summary>
    /// Internal timer for managing footstep delays.
    /// </summary>
    private float _timer;

    /// <summary>
    /// Called during the update phase to evaluate and trigger footstep sounds.
    /// </summary>
    public override void OnUpdateView()
    {
      if (PredictedFrame.TryGet<KCC2D>(EntityRef, out var kcc))
      {
        // Check if the entity is grounded and moving faster than the velocity threshold.
        if (kcc._state == KCCState.GROUNDED && Mathf.Abs(kcc.KinematicHorizontalSpeed.AsFloat) > VelocityThreshold)
        {
          _timer -= Time.deltaTime;
          if (_timer <= 0)
          {
            PlayFootstep();
          }
        }
      }
    }

    /// <summary>
    /// Plays the footstep sound and resets the delay timer.
    /// </summary>
    private void PlayFootstep()
    {
      _timer = StepsDelay;
      AudioController.OnFootStep();
    }
  }
}