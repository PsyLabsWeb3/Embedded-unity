namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  /// <summary>
  ///   The <c>BulletSystem</c> handles all bullet entity interactions, such as:
  ///    life cycle, movement and collision (via raycast);
  /// </summary>
  [Preserve]
  public unsafe class BulletSystem : SystemMainThreadFilter<BulletSystem.Filter>
  {
    /// <summary>
    /// The <c>Filter</c> struct represents the components required for the system's operations,
    /// including an entity reference, it's Transform2D and BulletFields.
    /// </summary>
    public struct Filter
    {
      /// <summary>
      /// The reference to the entity being processed.
      /// </summary>
      public EntityRef Entity;
      /// <summary>
      /// Pointer to the entity's Transform2D component.
      /// </summary>
      public Transform2D* Transform;
      /// <summary>
      /// Pointer to the entity's BulletFields component.
      /// </summary>
      public BulletFields* BulletFields;
    }

    /// <summary>
    /// It is called for each entity that matches the required components.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, its Transform2D and BulletsFields.</param>
    public override void Update(Frame frame, ref Filter filter)
    {
      if (CheckRaycastCollision(frame, ref filter))
      {
        return;
      }
      
      if (frame.Exists(filter.BulletFields->Source) == false)
      {
        frame.Destroy(filter.Entity);
        return;
      }
      UpdateBulletPosition(frame, ref filter);
      CheckBulletDistance(frame, ref filter);
    }

    /// <summary>
    /// Check if the bullet is to far from the initial position, performing the bullet actions when needed.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, its Transform2D and BulletsFields.</param>
    private void CheckBulletDistance(Frame frame, ref Filter filter)
    {
      var bulletFields = filter.BulletFields;
      var sourcePosition = frame.Unsafe.GetPointer<Transform2D>(bulletFields->Source)->Position;
      var distanceSquared = FPVector2.DistanceSquared(filter.Transform->Position, sourcePosition);
      
      var bulletData = frame.FindAsset(bulletFields->BulletData);
      bool bulletIsTooFar = FPMath.Sqrt(distanceSquared) > bulletData.Range;

      if (bulletIsTooFar)
      {
        // Applies polymorphic behavior on the bullet action
        bulletData.BulletAction(frame, filter.Entity, EntityRef.None);
      }
    }

    /// <summary>
    /// Change the bullet position using it's direction.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, its Transform2D and BulletsFields.</param>
    private void UpdateBulletPosition(Frame frame, ref Filter filter)
    {
      filter.Transform->Position += filter.BulletFields->Direction * frame.DeltaTime;
    }

    /// <summary>
    /// Check if the bullet is colliding, performs the bullet actions when needed.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, its Transform2D and BulletsFields.</param>
    private bool CheckRaycastCollision(Frame frame, ref Filter filter)
    {
      var bulletFields = filter.BulletFields;
      var bulletTransform = filter.Transform;
      
      if (bulletFields->Direction.Magnitude <= FP._0)
      {
        return false;
      }

      var futurePosition = bulletTransform->Position + bulletFields->Direction * frame.DeltaTime;
      var bulletData = frame.FindAsset(bulletFields->BulletData);

      var futurePositionDistance = FPVector2.DistanceSquared(bulletTransform->Position, futurePosition);
      if (futurePositionDistance <= bulletData.CollisionCheckThreshold)
      {
        return false;
      }

      Physics2D.HitCollection hits = frame.Physics2D.LinecastAll(bulletTransform->Position, futurePosition, -1, QueryOptions.HitAll | QueryOptions.ComputeDetailedInfo);
      for (int i = 0; i < hits.Count; i++)
      {
        var entity = hits[i].Entity;
        if (entity != EntityRef.None && frame.Has<Status>(entity) && entity != bulletFields->Source)
        {
          if (frame.Get<Status>(entity).IsDead)
          {
            continue;
          }

          bulletTransform->Position = hits[i].Point;

          // Applies polymorphic behavior on the bullet action
          bulletData.BulletAction(frame, filter.Entity, entity);
          return true;
        }

        if (entity == EntityRef.None)
        {
          bulletTransform->Position = hits[i].Point;

          // Applies polymorphic behavior on the bullet action
          bulletData.BulletAction(frame, filter.Entity, EntityRef.None);
          return true;
        }
      }
      return false;
    }
  }
}