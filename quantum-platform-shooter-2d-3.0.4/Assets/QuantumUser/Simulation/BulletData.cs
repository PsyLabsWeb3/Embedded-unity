namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine;
  
  /// <summary>
  /// The <c>BulletData</c> is a Polymorphic data asset for store bullets information and custom logic
  /// </summary>
  public abstract class BulletData : AssetObject
  {
#if QUANTUM_UNITY
    [Header("View Configuration")]
    [Tooltip("GameObject spawned when a bullet gets destroyed.")]
    public GameObject BulletDestroyFxGameObject;
    [Tooltip("AudioConfiguration that plays when a bullet gets destroyed.")]
    public PlatformShooter2D.AudioConfiguration BulletDestroyAudioInfo;
#endif
    
    [Header("Simulation configuration")]
    [Tooltip("Prototype reference to spawn bullet projectiles")]
    public AssetRef<EntityPrototype> BulletPrototype;
    [Tooltip("Damage applied in the target character.")]
    public FP Damage;
    [Tooltip("Maximum distance traveled before the bullet gets destroyed.")]
    public FP Range;
    [Tooltip("Minimum distance the bullet needs to travel before checking for a collision.")]
    public FP CollisionCheckThreshold = FP._0_01;

    /// <summary>
    /// Post hit effects should happen on this function call.
    /// Eg.: Explosions, damage calculations, etc.
    /// </summary>
    /// <param name="frame">The Quantum game Frame</param>
    /// <param name="bullet">The bullet EntityRef that triggered the function call</param>
    /// <param name="targetCharacter">The target of the bullet (it EntityRef.None when hitting a static collider)</param>
    public virtual unsafe void BulletAction(Frame frame, EntityRef bullet, EntityRef targetCharacter)
    {
    }
  }
}