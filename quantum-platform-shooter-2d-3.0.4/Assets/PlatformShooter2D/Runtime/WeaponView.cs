namespace PlatformShooter2D
{
  using UnityEngine;

  /// <summary>
  /// Represents the visual and functional aspects of a weapon, including hand positions and shooting effects.
  /// </summary>
  public class WeaponView : MonoBehaviour
  {
    /// <summary>
    /// Transform representing the weapon's right-hand grip position.
    /// </summary>
    public Transform RightHand;

    /// <summary>
    /// Transform representing the weapon's left-hand grip position.
    /// </summary>
    public Transform LeftHand;

    /// <summary>
    /// Transform used to indicate the weapon's look direction (e.g., aiming or firing direction).
    /// </summary>
    public Transform LookDir;

    /// <summary>
    /// Particle system for the weapon's muzzle flash effect.
    /// </summary>
    public ParticleSystem Muzzle;

    /// <summary>
    /// Triggers the shooting effect by stopping and restarting the muzzle flash particle system.
    /// </summary>
    public void ShootFx()
    {
      if (Muzzle != null)
      {
        Muzzle.Stop();
        Muzzle.Play();
      }
    }
  }
}