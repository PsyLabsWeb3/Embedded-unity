namespace PlatformShooter2D
{
  using UnityEngine;
  using Quantum;

  /// <summary>
  /// Handles unparenting a trail effect from its parent when the associated bullet is destroyed.
  /// </summary>
  public class UnparentTrailOnDestroy : QuantumSceneViewComponent
  {
    /// <summary>
    /// The particle system effect representing the trail.
    /// </summary>
    public ParticleSystem Effect;

    /// <summary>
    /// The Quantum entity view representing the bullet.
    /// </summary>
    public QuantumEntityView BulletView;

    /// <summary>
    /// The entity reference of the character that owns the bullet.
    /// </summary>
    private EntityRef _characterEntityRef;

    /// <summary>
    /// The original parent transform of the trail effect.
    /// </summary>
    private Transform _parent;

    /// <summary>
    /// Subscribes to events and initializes references when the object starts.
    /// </summary>
    /// <param name="frame">The current Quantum frame.</param>
    public override void OnActivate(Frame frame)
    {
      // Subscribe to the bullet destruction event.
      QuantumEvent.Subscribe<EventOnBulletDestroyed>(this, OnBulletDestroyed);

      // Retrieve the bullet entity and its source.
      BulletView = GetComponentInParent<QuantumEntityView>();
      if (PredictedFrame.Exists(BulletView.EntityRef))
      {
        var bulletFields = PredictedFrame.Get<BulletFields>(BulletView.EntityRef);
        _characterEntityRef = bulletFields.Source;
        _parent = transform.parent;
        transform.parent = null;
      }
    }

    /// <summary>
    /// Updates the trail position to follow the original parent.
    /// </summary>
    public override void OnUpdateView()
    {
      if (_parent != null)
      {
        transform.position = _parent.position;
      }
    }

    /// <summary>
    /// Stops the trail effect and repositions it when the bullet is destroyed.
    /// </summary>
    /// <param name="eventData">The data of the bullet destruction event.</param>
    private void OnBulletDestroyed(EventOnBulletDestroyed eventData)
    {
      // Ignore if the event is not related to the current bullet.
      if (_characterEntityRef != eventData.Owner)
      {
        return;
      }

      // Stop the particle effect and reposition it to the destruction point.
      Effect.Stop();
      Vector3 position = eventData.BulletPosition.ToUnityVector3();
      Effect.transform.position = position;
    }
  }
}