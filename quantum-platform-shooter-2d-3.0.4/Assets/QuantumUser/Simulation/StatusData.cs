using UnityEngine.Serialization;

namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine;
  /// <summary>
  /// The <c>StatusData</c> is a data asset for store initial values for character status component.
  /// </summary>
  public class StatusData : AssetObject
  {
    [Tooltip("Character health value when full.")]
    public FP MaxHealth;
    [Tooltip("Time until respawn after destruction.")]
    public FP RespawnTime;
    [Tooltip("Time until starts regen character health.")]
    public FP TimeUntilRegen;
    [Tooltip("Velocity of character health regeneration.")]
    public FP RegenRate;
    [Tooltip("Time before character can be hit after respawn.")]
    public FP InvincibleTime;
    [Tooltip("Time to wait in seconds until destroying the character entity after the player disconnects.")]
    public FP TimeToDisconnect = 1;
    [Tooltip("Minimum values to consider character damage.")]
    public FP MinimumDamage = FP._1;
  }
}