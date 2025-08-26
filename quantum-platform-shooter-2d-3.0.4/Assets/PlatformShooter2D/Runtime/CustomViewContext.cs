using UnityEngine.Serialization;

namespace PlatformShooter2D
{
  using Photon.Deterministic;
  using Quantum;
  using UnityEngine;

  /// <summary>
  /// Provides a custom context for Quantum views, storing local player-specific data.
  /// </summary>
  public class CustomViewContext : MonoBehaviour, IQuantumViewContext
  {
    /// <summary>
    /// Reference to the local player's character view.
    /// </summary>
    public CharacterView LocalCharacterView;

    /// <summary>
    /// Stores the last movement direction of the local player as a FPVector2.
    /// </summary>
    public FPVector2 LocalCharacterLastDirection;
  }
}