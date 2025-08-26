namespace Quantum
{
  using Photon.Deterministic;

  /// <summary>
  ///   The <c>WeaponHelper</c> provides an easy way to access the position offset to spawn bullets.
  /// </summary>
  public static unsafe class WeaponHelper
  {
    /// <summary>
    /// Post hit effects should happen on this function call.
    /// 
    /// Eg.: Explosions, damage calculations, etc.
    /// </summary>
    /// <param name="weaponData">WeaponData instance.</param>
    /// <param name="direction">The direction the bullet is being shot</param>
    public static FPVector2 GetFireSpotWorldOffset(WeaponData weaponData, FPVector2 direction)
    {
      FPVector2 positionOffset = weaponData.PositionOffset;
      FPVector2 firespotVector = weaponData.FireSpotOffset;
      return positionOffset + direction + firespotVector;
    }
  }
}