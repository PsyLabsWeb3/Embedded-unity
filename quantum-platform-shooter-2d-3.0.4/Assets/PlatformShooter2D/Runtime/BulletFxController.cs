namespace PlatformShooter2D
{
  using UnityEngine;
  using Quantum;

  /// <summary>
  /// This behavior handles events related to bullets, such as destruction effects and explosions.
  /// </summary>
  public sealed class BulletFxController : QuantumSceneViewComponent
  {
    /// <summary>
    /// Called during initialization. Subscribes to the bullet destruction event.
    /// </summary>
    public override void OnInitialize()
    {
      QuantumEvent.Subscribe<EventOnBulletDestroyed>(this, OnBulletDestroyed);
    }

    /// <summary>
    /// Handles the bullet destruction event by spawning destruction effects.
    /// </summary>
    /// <param name="eventData">Data associated with the bullet destruction event.</param>
    private void OnBulletDestroyed(EventOnBulletDestroyed eventData)
    {
      // Convert the bullet position and direction from Quantum to Unity's vector format.
      Vector3 position = eventData.BulletPosition.ToUnityVector3();
      Vector3 direction = eventData.BulletDirection.ToUnityVector3();

      // Retrieve bullet data and fetch the destruction effect prefab.
      var bulletData = QuantumUnityDB.GetGlobalAsset(eventData.BulletData);
      var fxPrefab = bulletData.BulletDestroyFxGameObject;

      // Instantiate the destruction effect at the bullet's position and orientation.
      Instantiate(fxPrefab, position, Quaternion.LookRotation(-direction));
    }
  }
}