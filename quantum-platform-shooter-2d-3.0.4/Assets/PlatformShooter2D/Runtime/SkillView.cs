namespace PlatformShooter2D
{
  using Quantum;
  using UnityEngine;
  using System.Collections;

  /// <summary>
  /// Handles the visual effects associated with skill actions such as activation, hits, and explosions.
  /// </summary>
  public class SkillView : QuantumSceneViewComponent
  {
    /// <summary>
    /// The particle effect to play when a skill explodes.
    /// </summary>
    public ParticleSystem EffectExplosionPrefab;

    /// <summary>
    /// The particle effect to play when a skill hits a target.
    /// </summary>
    public ParticleSystem EffectHitPrefab;

    /// <summary>
    /// The particle effect to play during skill activation.
    /// </summary>
    public ParticleSystem EffectPrefab;

    /// <summary>
    /// Subscribes to events for skill activation and skill hitting a target.
    /// </summary>
    public override void OnInitialize()
    {
      QuantumEvent.Subscribe<EventOnSkillHitTarget>(this, HitEffect);
      QuantumEvent.Subscribe<EventOnSkillActivated>(this, SkillActivated);
    }

    /// <summary>
    /// Triggers the explosion effect at the skill activation position.
    /// </summary>
    /// <param name="eventData">The event data containing information about the skill activation.</param>
    private void SkillActivated(EventOnSkillActivated eventData)
    {
      Instantiate(EffectExplosionPrefab, eventData.SkillPosition.ToUnityVector3(), Quaternion.identity);
    }

    /// <summary>
    /// Triggers the hit effect between the skill's initial and target positions.
    /// </summary>
    /// <param name="eventData">The event data containing information about the skill hitting a target.</param>
    private void HitEffect(EventOnSkillHitTarget eventData)
    {
      Frame frame = eventData.Game.Frames.Predicted;
      var characterPosition = frame.Get<Transform2D>(eventData.Target).Position;
      var initialPosition = eventData.SkillPosition.ToUnityVector3();
      var finalPosition = characterPosition.ToUnityVector3();
      StartCoroutine(HitEffectCoroutine(initialPosition, finalPosition));
    }

    /// <summary>
    /// Plays the visual effects for the skill traveling from its position to the target position, 
    /// and the hit effect at the final position.
    /// </summary>
    /// <param name="initialPosition">The position of the skill effect.</param>
    /// <param name="finalPosition">The final position where the skill hits the target.</param>
    /// <returns>An IEnumerator to control the effect coroutine.</returns>
    private IEnumerator HitEffectCoroutine(Vector3 initialPosition, Vector3 finalPosition)
    {
      var obj = Instantiate(EffectPrefab, initialPosition, Quaternion.identity);
      yield return null;
      obj.transform.position = finalPosition;
      Instantiate(EffectHitPrefab, finalPosition, Quaternion.identity);
    }
  }
}