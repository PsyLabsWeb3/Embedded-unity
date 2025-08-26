namespace PlatformShooter2D
{
  using UnityEngine;

  /// <summary>
  /// Handles the creation of smoke effects for the bazooka weapon.
  /// </summary>
  public class CreateBazookaSmoke : MonoBehaviour
  {
    /// <summary>
    /// Prefab used to create the smoke effect.
    /// </summary>
    public GameObject SmokePrefab;

    /// <summary>
    /// Instantiates the smoke prefab as a child of the bazooka's parent object.
    /// </summary>
    public void OnCreateSmoke()
    {
      Instantiate(SmokePrefab, transform.parent);
    }
  }
}