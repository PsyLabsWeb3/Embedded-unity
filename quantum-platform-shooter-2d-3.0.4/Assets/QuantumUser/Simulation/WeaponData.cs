namespace Quantum
{
  using Photon.Deterministic;
#if QUANTUM_UNITY
  using UnityEngine;
#endif

  /// <summary>
  /// The <c>WeaponData</c> is a Polymorphic data asset for store weapons information and custom logic
  /// </summary>
  [System.Serializable]
  public class WeaponData : AssetObject
  {
#if QUANTUM_UNITY
    [Header("View Configuration")] 
    [Tooltip("Audio played when the gun fires.")]
    public PlatformShooter2D.AudioConfiguration ShootAudioInfo;
    [Tooltip("Sprite displayed on UI.")]
    public Sprite UIIcon;
    [Tooltip("Weapon model.")]
    public GameObject Prefab;
#endif
    
    [Header("Simulation configuration")]
    [Tooltip("Speed the weapon  performs.")]
    public FP FireRate;
    [Tooltip("Speed of the bullet spawned by the weapon.")]
    public FP ShootForce;
    [Tooltip("Amount of bullets the weapon stores when full loaded.")]
    public int MaxAmmo;
    [Tooltip("Time spend during weapon recharge.")]
    public FP RechargeTimer;
    [Tooltip("Time delay before starting recharge weapon.")]
    public FP TimeToRecharge;
    [Tooltip("Position offset used in the fire VFX.")]
    public FPVector2 FireSpotOffset;
    [Tooltip("Position offset used to spawn bullets.")]
    public FPVector2 PositionOffset;

    [Tooltip("Asset reference containing the bullet data.")]
    public AssetRef<BulletData> BulletData;
  }
}