namespace PlatformShooter2D
{
  using UnityEngine;

  /// <summary>
  /// Handles weapon animation root adjustments based on the local position of the SHRoot transform.
  /// </summary>
  public class WeaponAnimationRoot : MonoBehaviour
  {
    /// <summary>
    /// The root transform used for animation adjustments.
    /// </summary>
    public Transform SHRoot;

    /// <summary>
    /// The calculated weapon offset based on the SHRoot's position.
    /// </summary>
    public Vector3 WeaponOffset;

    /// <summary>
    /// The minimum Y-position of SHRoot that contributes to offset calculations.
    /// </summary>
    private float _minShRoot = 0.7f;

    /// <summary>
    /// Updates the weapon offset each frame based on the SHRoot's Y-position.
    /// </summary>
    void Update()
    {
      WeaponOffset = new Vector3(0, SHRoot.localPosition.y - _minShRoot, 0);
    }
  }
}